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
        // ---
        CancellationToken cancellationToken = default)
    {
        var response = await interaction.DispatchAsync(
            new CreateSelectOrganisationSessionRequest {
                ClientId = scopedSession.Application.ClientId,
                UserId = request.UserId,
                Prompt = request.Prompt,
                Filter = request.Filter,
                AllowCancel = request.AllowCancel,
                CallbackUrl = request.CallbackUrl,
            }, cancellationToken
        ).To<CreateSelectOrganisationSessionResponse>();

        return new CreateSelectOrganisationSessionApiResponse {
            RequestId = response.RequestId,
            HasOptions = response.HasOptions,
            Url = response.Url,
        };
    }
}
