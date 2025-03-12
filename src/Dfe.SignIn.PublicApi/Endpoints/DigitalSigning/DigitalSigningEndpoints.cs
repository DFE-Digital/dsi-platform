using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.PublicModels.PublicApiSigning;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.AspNetCore.Mvc;
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
    [Produces("application/json", Type = typeof(WellKnownPublicKeyListing))]
    public static IResult GetKeys(
        IOptions<ApplicationOptions> optionsAccessor)
    {
        string publicKeysJson = optionsAccessor.Value.PublicKeysJson;
        if (string.IsNullOrWhiteSpace(publicKeysJson)) {
            throw new InvalidOperationException("Missing configuration 'ApplicationOptions.PublicKeysJson'.");
        }

        return Results.Text(publicKeysJson, "application/json");
    }
}
