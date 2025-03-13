using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation.Models;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

public static partial class SelectOrganisationEndpoints
{
    /// <summary>
    /// Create a URL which the end-user can use to select an organisation.
    /// </summary>
    public static async Task<CreateSelectOrganisationSession_PublicApiResponse> PostSelectOrganisationSession(
        [FromBody] CreateSelectOrganisationSession_PublicApiRequest apiRequest,
        IScopedSessionReader scopedSession,
        IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse> createSelectOrganisationSession,
        IMapper mapper)
    {
        var response = await createSelectOrganisationSession.InvokeAsync(
            mapper.Map<CreateSelectOrganisationSessionRequest>(apiRequest) with {
                ClientId = scopedSession.Application.ClientId,
            }
        );
        return mapper.Map<CreateSelectOrganisationSession_PublicApiResponse>(response);
    }
}
