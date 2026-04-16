using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.Applications;

/// <summary>
/// Endpoints for the "get application roles" feature.
/// </summary>
public static partial class ApplicationEndpoints
{
    /// <summary>
    /// Registers all endpoints for the "get application roles" feature.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseApplicationEndpoints(this WebApplication app)
    {
        app.MapPost("services/{clientId}/roles", GetApplicationRoles);
    }
}
