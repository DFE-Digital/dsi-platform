using System.IdentityModel.Tokens.Jwt;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.SignIn.PublicApi.BearerTokenAuth;

/// <summary>
/// Protects routes by ensuring the presence of a valid bearer token
/// </summary>
public class BearerTokenAuthMiddleware
{
    private readonly RequestDelegate next;
    private readonly BearerTokenOptions options;

    /// <summary>
    /// Construct and instance of BearerTokenAuthMiddleware.
    /// </summary>
    /// <param name="next">The next RequestDelegate to process after the middleware.</param>
    /// <param name="optionsAccessor">Accesses options to be used by the BearerTokenAuthMiddleware.</param>
    public BearerTokenAuthMiddleware(
        RequestDelegate next,
        IOptions<BearerTokenOptions> optionsAccessor)
    {
        this.next = next;
        this.options = optionsAccessor.Value;
    }

    private record ErrorResponse()
    {
        public bool Success { get; } = false;
        public required string Message { get; set; }
    }

    private static async Task SendErrorResponseAsync(string message, HttpContext context, int statusCode = StatusCodes.Status401Unauthorized)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse { Message = message });
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader)) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await SendErrorResponseAsync("Missing Authorization header", context);
            return;
        }

        var authValue = authHeader.ToString();
        if (string.IsNullOrEmpty(authValue) || !authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
            await SendErrorResponseAsync("Malformed Authorization header. Should be bearer {token}", context);
            return;
        }

        var token = authValue["Bearer ".Length..].Trim();
        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token)) {
            await SendErrorResponseAsync("Invalid token provided", context);
            return;
        }

        var jwtToken = tokenHandler.ReadJwtToken(token);
        if (string.IsNullOrEmpty(jwtToken.Issuer)) {
            await SendErrorResponseAsync("Missing or invalid iss claim", context, StatusCodes.Status403Forbidden);
            return;
        }

        var serviceProvider = context.RequestServices;
        IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse> service =
            serviceProvider.GetRequiredService<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>();

        var response = await service.InvokeAsync(new GetApplicationByClientIdRequest { ClientId = jwtToken.Issuer });

        if (response is null || response.Application is null) {
            await SendErrorResponseAsync("Unknown issuer", context, StatusCodes.Status403Forbidden);
            return;
        }

        if (string.IsNullOrEmpty(response.Application.ApiSecret)) {
            await SendErrorResponseAsync("Your client is not authorized to use this api", context, StatusCodes.Status403Forbidden);
            return;
        }

        // Attempt to grab Expires and Not Before, as this will be used to determine
        // whether JWT validation sets ValidateLifeTime true/false.  Possibly not required
        // at all, and ValidateLifeTime should be false.
        var hasExp = jwtToken.Claims.Any(c => c.Type == "exp");
        var hasNbf = jwtToken.Claims.Any(c => c.Type == "nbf");

        // Convert the string into a byte-array
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(response.Application.ApiSecret);

        keyBytes = HmacKeyNormalizer.NormalizeHmacSha256Key(keyBytes);

        var tokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = response.Application.ClientId,
            ValidateAudience = true,
            ValidAudience = this.options.ValidAudience,
            ValidateLifetime = hasExp || hasNbf,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromSeconds(this.options.ClockSkewSeconds),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        };

        try {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out var _);

            var scopedSession = serviceProvider.GetRequiredService<IScopedSessionWriter>();
            scopedSession.Application = response.Application;
        }
        catch (SecurityTokenExpiredException) {
            await SendErrorResponseAsync("jwt expired", context, StatusCodes.Status403Forbidden);
            return;
        }
        catch {
            await SendErrorResponseAsync("Your client is not authorized to use this api", context, StatusCodes.Status403Forbidden);
            return;
        }

        await this.next(context);
    }
}
