using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.Organisations;
using Dfe.SignIn.PublicApi.Contracts.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    /// <summary>
    /// Queries a specific organisation of a user.
    /// </summary>
    /// <returns>
    ///   <para>200 with the organisation details (or null if not found).</para>
    ///   <para>403 when the client application cannot be found.</para>
    /// </returns>
    public static async Task<Results<Ok<QueryUserOrganisationApiResponse>, ForbidHttpResult>> PostQueryUserOrganisation(
        Guid userId,
        Guid organisationId,
        // ---
        [FromBody] QueryUserOrganisationApiRequestBody request,
        // ---
        IClientSession scopedSession,
        IInteractionDispatcher interaction)
    {
        try {
            var filteredOrganisationsResponse = await interaction.DispatchAsync(
                new FilterOrganisationsForUserRequest {
                    ClientId = scopedSession.ClientId,
                    UserId = userId,
                    Filter = request.Filter,
                }
            ).To<FilterOrganisationsForUserResponse>();

            var organisation = filteredOrganisationsResponse.FilteredOrganisations
                .FirstOrDefault(organisation => organisation.Id == organisationId);

            return TypedResults.Ok(new QueryUserOrganisationApiResponse {
                UserId = userId,
                Organisation = organisation is null ? null : new OrganisationDetails {
                    Id = organisation.Id,
                    Name = organisation.Name,
                    LegalName = organisation.LegalName
                }
            });
        }
        catch (ApplicationNotFoundException) {
            return TypedResults.Forbid();
        }
    }
}
