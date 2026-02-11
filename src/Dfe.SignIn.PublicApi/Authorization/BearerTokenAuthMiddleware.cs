using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.PublicApi.Contracts.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.SignIn.PublicApi.Authorization;

/// <summary>
/// Protects routes by ensuring the presence of a valid bearer token
/// </summary>
/// <param name="next">The next RequestDelegate to process after the middleware.</param>
/// <param name="optionsAccessor">Accesses options to be used by the BearerTokenAuthMiddleware.</param>
public sealed class BearerTokenAuthMiddleware(
    RequestDelegate next,
    IOptions<BearerTokenOptions> optionsAccessor)
{
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    private sealed record ErrorResponse
    {
        public bool Success { get; } = false;
        public required string Message { get; set; }
    }

    private static async Task SendErrorResponseAsync(string message, HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse { Message = message });
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context)
    {
        var jwt = await this.ReadTokenFromAuthorizationHeaderAsync(context);
        if (jwt is null) {
            return;
        }

        var clientConfiguration = await GetApplicationApiConfigurationAsync(context, jwt.Issuer);
        if (clientConfiguration is null) {
            return;
        }

        try {
            this.ValidateTokenAsync(jwt, clientConfiguration);

            var scopedSession = context.RequestServices.GetRequiredService<IClientSessionWriter>();
            scopedSession.ClientId = clientConfiguration.ClientId;
        }
        catch (SecurityTokenExpiredException) {
            await SendErrorResponseAsync("jwt expired", context, StatusCodes.Status403Forbidden);
            return;
        }
        catch (SecurityTokenException) {
            await SendErrorResponseAsync("Your client is not authorized to use this api", context, StatusCodes.Status403Forbidden);
            return;
        }

        await next(context);
    }

    private async Task<JwtSecurityToken?> ReadTokenFromAuthorizationHeaderAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader)) {
            await SendErrorResponseAsync("Missing Authorization header", context, StatusCodes.Status401Unauthorized);
            return null;
        }

        string authValue = authHeader.ToString();
        if (string.IsNullOrEmpty(authValue) || !authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
            await SendErrorResponseAsync("Malformed Authorization header. Should be bearer {token}", context, StatusCodes.Status401Unauthorized);
            return null;
        }

        string token = authValue["Bearer ".Length..].Trim();
        if (!this.tokenHandler.CanReadToken(token)) {
            await SendErrorResponseAsync("Invalid token provided", context, StatusCodes.Status401Unauthorized);
            return null;
        }

        var jwt = this.tokenHandler.ReadJwtToken(token);
        if (string.IsNullOrEmpty(jwt.Issuer)) {
            await SendErrorResponseAsync("Missing or invalid iss claim", context, StatusCodes.Status403Forbidden);
            return null;
        }

        return jwt;
    }

    private static async Task<ApplicationApiConfiguration?> GetApplicationApiConfigurationAsync(HttpContext context, string clientId)
    {
        var interaction = context.RequestServices.GetRequiredService<IInteractionDispatcher>();

        GetApplicationApiConfigurationResponse configurationResponse;
        try {
            configurationResponse = await interaction.DispatchAsync(
                new GetApplicationApiConfigurationRequest {
                    ClientId = clientId,
                }
            ).To<GetApplicationApiConfigurationResponse>();

            if (string.IsNullOrEmpty(configurationResponse.Configuration.ApiSecret)) {
                await SendErrorResponseAsync("Your client is not authorized to use this api", context, StatusCodes.Status403Forbidden);
                return null;
            }

            return configurationResponse.Configuration;
        }
        catch (ApplicationNotFoundException) {
            await SendErrorResponseAsync("Unknown issuer", context, StatusCodes.Status403Forbidden);
            return null;
        }
    }

    private void ValidateTokenAsync(JwtSecurityToken jwt, ApplicationApiConfiguration clientConfiguration)
    {
        var options = optionsAccessor.Value;

        // Attempt to grab Expires and Not Before, as this will be used to determine
        // whether JWT validation sets ValidateLifeTime true/false.  Possibly not required
        // at all, and ValidateLifeTime should be false.
        var hasExp = jwt.Claims.Any(c => c.Type == "exp");
        var hasNbf = jwt.Claims.Any(c => c.Type == "nbf");

        byte[] keyBytes = HmacKeyNormalizer.NormalizeHmacSha256Key(
            Encoding.UTF8.GetBytes(clientConfiguration.ApiSecret)
        );

        var tokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = clientConfiguration.ClientId,
            ValidateAudience = true,
            ValidAudience = options.ValidAudience,
            ValidateLifetime = hasExp || hasNbf,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        };

        this.tokenHandler.ValidateToken(jwt.RawData, tokenValidationParameters, out var _);
    }
}
