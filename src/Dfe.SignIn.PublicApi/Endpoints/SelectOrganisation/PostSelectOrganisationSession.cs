using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;

public static partial class SelectOrganisationEndpoints
{
    /// <summary>
    /// Create a URL which the end-user can use to select an organisation.
    /// </summary>
    /// <returns>
    ///   <para>201 with the session URL and whether options are available.</para>
    ///   <para>403 when the client application cannot be found.</para>
    /// </returns>
    public static async Task<Results<Ok<CreateSelectOrganisationSessionApiResponse>, ForbidHttpResult>> PostSelectOrganisationSession(
        [FromBody] CreateSelectOrganisationSessionApiRequestBody request,
        // ---
        IClientSession session,
        IInteractionDispatcher interaction)
    {
        try {
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

            return TypedResults.Ok(new CreateSelectOrganisationSessionApiResponse {
                RequestId = response.RequestId,
                HasOptions = response.HasOptions,
                Url = response.Url,
            });
        }
        catch (ApplicationNotFoundException) {
            return TypedResults.Forbid();
        }
    }
}
