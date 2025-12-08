using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;
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
        IClientSession session,
        IInteractionDispatcher interaction)
    {
        var response = await interaction.DispatchAsync(
            new CreateSelectOrganisationSessionRequest {
                ClientId = session.ClientId,
                UserId = request.UserId,
                Prompt = request.Prompt,
                Filter = request.Filter,
                AllowCancel = request.AllowCancel,
                CallbackUrl = request.CallbackUrl,
            }
        ).To<CreateSelectOrganisationSessionResponse>();

        return new CreateSelectOrganisationSessionApiResponse {
            RequestId = response.RequestId,
            HasOptions = response.HasOptions,
            Url = response.Url,
        };
    }
}
