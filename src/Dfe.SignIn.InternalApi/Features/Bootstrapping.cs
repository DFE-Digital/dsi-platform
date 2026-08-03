using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.InternalApi.Features.Users;

namespace Dfe.SignIn.InternalApi.Features;

/// <summary>
/// Bootstrapping class to map feature endpoints for the application.
/// </summary>
[ExcludeFromCodeCoverage]
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

    /// <summary>
    /// Adds the feature services to the service collection, including user-related services.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFeaturesServices(this IServiceCollection services)
    {
        services.AddUserServices();
        return services;
    }
}
