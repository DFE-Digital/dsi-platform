using Dfe.SignIn.InternalApi.Features.Users;

namespace Dfe.SignIn.InternalApi.Features;

/// <summary>
/// Bootstrapping class to map feature endpoints for the application.
/// </summary>
public static class Bootstrapping
{
    /// <summary>
    /// Maps the feature endpoints for the application, including user-related endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void MapFeaturesEndpoints(this WebApplication app)
    {
        app.MapUsersEndpoints();
    }
}
