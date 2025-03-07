using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Models.Requests;

namespace Dfe.SignIn.PublicApi.Endpoints;

/// <summary>
/// Endpoints for the "select organisation" feature.
/// </summary>
public static class SelectOrganisationEndpoints
{
    /// <summary>
    /// Registers all endpoints for the "select organisation" feature.
    /// </summary>
    /// <param name="app">The application instance.</param>
    public static void RegisterSelectOrganisationEndpoints(this WebApplication app)
    {
        app.MapPost("v2/select-organisation", async (
            CreateSelectOrganisationSessionApiRequest apiRequest,
            IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse> createSelectOrganisationSession,
            IMapper mapper
        ) => {
            var response = await createSelectOrganisationSession.InvokeAsync(
                mapper.Map<CreateSelectOrganisationSessionRequest>(apiRequest) with {
                    ClientId = "test"
                }
            );
            return response;
        });
    }
}
