using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;

/// <summary>
/// Endpoints for the digital signing feature.
/// </summary>
public static class DigitalSigningEndpoints
{
    /// <summary>
    /// Registers all endpoints for the digital signing feature.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseDigitalSigningEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("v2/.well-known/keys", GetKeys);
    }

    /// <summary>
    /// Gets the list of public keys to verify digitally signed payloads.
    /// </summary>
    public static string GetKeys(
        IOptions<ApplicationOptions> optionsAccessor)
    {
        string publicKeysJson = optionsAccessor.Value.PublicKeysJson;
        if (string.IsNullOrWhiteSpace(publicKeysJson)) {
            throw new InvalidOperationException("Missing configuration 'ApplicationOptions.PublicKeysJson'.");
        }
        return publicKeysJson;
    }
}
