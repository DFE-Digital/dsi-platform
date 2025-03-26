using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;

/// <summary>
/// Endpoints for the digital signing feature.
/// </summary>
public static partial class DigitalSigningEndpoints
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
}
