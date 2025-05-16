using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Endpoints for user feature.
/// </summary>
public static partial class UserEndpoints
{
    /// <summary>
    /// Registers all endpoints for user features.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseUserEndpoints(this WebApplication app)
    {
        app.MapPost("v2/users/{userId}/organisations/{organisationId}/query", PostQueryUserOrganisation);
    }
}
