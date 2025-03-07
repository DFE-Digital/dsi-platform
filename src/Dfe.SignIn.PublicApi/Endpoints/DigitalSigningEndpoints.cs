namespace Dfe.SignIn.PublicApi.Endpoints;

/// <summary>
/// Endpoints for the digital signing feature.
/// </summary>
public static class DigitalSigningEndpoints
{
    /// <summary>
    /// Registers all endpoints for the digital signing feature.
    /// </summary>
    /// <param name="app">The application instance.</param>
    public static void RegisterDigitalSigningEndpoints(this WebApplication app)
    {
        app.MapGet("v2/.well-known/keys", () => {
            return 0;
        });
    }
}
