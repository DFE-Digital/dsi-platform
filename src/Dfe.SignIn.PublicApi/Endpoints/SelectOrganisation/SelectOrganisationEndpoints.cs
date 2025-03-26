using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

/// <summary>
/// Endpoints for the "select organisation" feature.
/// </summary>
public static partial class SelectOrganisationEndpoints
{
    /// <summary>
    /// Registers all endpoints for the "select organisation" feature.
    /// </summary>
    /// <param name="app">The application instance.</param>
    [ExcludeFromCodeCoverage]
    public static void UseSelectOrganisationEndpoints(this WebApplication app)
    {
        app.MapPost("v2/select-organisation", PostSelectOrganisationSession);
    }
}
