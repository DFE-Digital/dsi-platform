using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;

public static partial class DigitalSigningEndpoints
{
    /// <summary>
    /// Gets the list of public keys to verify digitally signed payloads.
    /// </summary>
    [Produces("application/json", Type = typeof(WellKnownPublicKeyListing))]
    public static IResult GetKeys(IOptions<ApplicationOptions> optionsAccessor)
    {
        string publicKeysJson = optionsAccessor.Value.PublicKeysJson;
        if (string.IsNullOrWhiteSpace(publicKeysJson)) {
            throw new InvalidOperationException("Missing configuration 'ApplicationOptions.PublicKeysJson'.");
        }

        return Results.Text(publicKeysJson, "application/json");
    }
}
