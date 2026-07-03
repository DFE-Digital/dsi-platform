using Dfe.SignIn.InternalApi.Feature.Applications;
using Dfe.SignIn.InternalApi.Feature.Users;

namespace Dfe.SignIn.InternalApi.Feature;

/// <summary>
/// Bootstrapping class to map feature endpoints for the application.
/// </summary>
public static class Bootstrapping
{
    /// <summary>
    /// Maps the feature endpoints for the application, including user-related endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        app.UseMyUserEndpoints();
        app.UseApplicationEndpoints();
    }
}
