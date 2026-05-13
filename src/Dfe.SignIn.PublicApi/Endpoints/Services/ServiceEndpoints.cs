using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.Services;

/// <summary>
/// Endpoints for service features.
/// </summary>
public static partial class ServiceEndpoints
{
    /// <summary>
    /// Registers all endpoints for service features.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseServiceEndpoints(this WebApplication app)
    {
        app.MapGet("services/{serviceId}/organisations/{organisationId}/users/{userId}", GetUserServiceAccessDetails);
    }
}
