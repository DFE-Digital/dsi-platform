using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Delegating handler which adds authorization header for requests to the public API.
/// </summary>
internal sealed class PublicApiBearerTokenHandler(
    IOptions<PublicApiOptions> optionsAccessor,
    IMemoryCache memoryCache
) : DelegatingHandler
{
    private static readonly object BearerTokenCacheKey = new();

    /// <inheritdoc/>
    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        request.Headers.Authorization = this.CreateAuthorizationHeader();
        return base.Send(request, cancellationToken);
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        request.Headers.Authorization = this.CreateAuthorizationHeader();

#if DEBUG
        // The assumption here is that all requests are going to the DEV public API instance.
        // Re-route all v2 endpoints to a local server for debugging.
        // TODO: In the future there will need to be a better way to manage this.
        var requestUri = request.RequestUri!;
        if (requestUri.IsAbsoluteUri && requestUri.AbsolutePath.StartsWith("/v2/")) {
            request.RequestUri = new Uri($"http://localhost:5086{request.RequestUri!.LocalPath}");
        }
#endif

        return base.SendAsync(request, cancellationToken);
    }

    internal AuthenticationHeaderValue CreateAuthorizationHeader()
    {
        return new AuthenticationHeaderValue("Bearer", this.GetAuthorizationToken());
    }

    private string GetAuthorizationToken()
    {
        if (!memoryCache.TryGetValue(BearerTokenCacheKey, out string? token)) {
            var options = this.VerifyPublicApiOptions();

            if (options.BearerTokenTtlInMinutes > 0) {
                var tokenExpirationUtc = DateTime.UtcNow.AddMinutes(options.BearerTokenTtlInMinutes);
                token = this.GenerateAuthorizationToken(options, tokenExpirationUtc);

                // Even though the token expires after the configured TTL; the memory
                // cache needs to expire sooner to allow a cross-over period.
                double cacheTtlInMinutes = Math.Max(
                    // Invalidate cache 5 minutes sooner than specified (could go negative).
                    options.BearerTokenTtlInMinutes - 5.0,
                    // If TTL is < 5 minutes then invalidate cache after half that.
                    options.BearerTokenTtlInMinutes / 2.0
                );

                var cacheExpiration = DateTimeOffset.UtcNow.AddMinutes(cacheTtlInMinutes);
                memoryCache.Set(BearerTokenCacheKey, token, absoluteExpiration: cacheExpiration);
            }
            else {
                // Token does not expire.
                token = this.GenerateAuthorizationToken(options, null);
                memoryCache.Set(BearerTokenCacheKey, token);
            }
        }
        return token!;
    }

    private PublicApiOptions VerifyPublicApiOptions()
    {
        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(options.ApiSecret)) {
            throw new InvalidOperationException("Invalid DfE Sign-in Public API secret.");
        }
        if (string.IsNullOrWhiteSpace(options.ClientId)) {
            throw new InvalidOperationException("Invalid DfE Sign-in Public API client ID.");
        }
        if (string.IsNullOrWhiteSpace(options.Audience)) {
            throw new InvalidOperationException("Invalid DfE Sign-in Public API service audience.");
        }

        return options;
    }

    private string GenerateAuthorizationToken(PublicApiOptions options, DateTime? expirationUtc)
    {
        byte[] key = Encoding.UTF8.GetBytes(options.ApiSecret);

        var tokenDescriptor = new SecurityTokenDescriptor {
            Audience = options.Audience,
            Issuer = options.ClientId,
            Expires = expirationUtc,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler {
            SetDefaultTimesOnTokenCreation = false,
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
