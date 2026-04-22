using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.Applications;

/// <summary>
/// A collection of endpoints related to service applications, including retrieving application roles.
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
        app.MapGet("services/{clientId}/roles", GetApplicationRoles)
            .WithName("GetApplicationRolesRequest")
            .WithTags("Applications")
            .Produces<IEnumerable<Contracts.Applications.ApplicationRoleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();
    }
}
