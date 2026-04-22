using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

/// <summary>
/// Endpoints for organisation features.
/// </summary>
public static partial class OrganisationEndpoints
{
    /// <summary>
    /// Registers all endpoints for Organisation features.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseOrganisationEndpoints(this WebApplication app)
    {
        app.MapGet("organisations/{ukprn}/users", GetUsersAtOrganisation);
    }
}
