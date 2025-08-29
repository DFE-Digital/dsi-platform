using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

public static partial class SelectOrganisationEndpoints
{
    /// <summary>
    /// Create a URL which the end-user can use to select an organisation.
    /// </summary>
    public static async Task<CreateSelectOrganisationSessionApiResponse> PostSelectOrganisationSession(
        [FromBody] CreateSelectOrganisationSessionApiRequestBody request,
        // ---
        IScopedSessionReader scopedSession,
        IInteractionDispatcher interaction,
        IMapper mapper,
        // ---
        CancellationToken cancellationToken = default)
    {
        var response = await interaction.DispatchAsync(
            mapper.Map<CreateSelectOrganisationSessionRequest>(request) with {
                ClientId = scopedSession.Application.ClientId,
            }, cancellationToken
        ).To<CreateSelectOrganisationSessionResponse>();
        return mapper.Map<CreateSelectOrganisationSessionApiResponse>(response);
    }
}
