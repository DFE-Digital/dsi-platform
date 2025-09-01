using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Client.Users;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    /// <summary>
    /// Queries a specific organisation of a user.
    /// </summary>
    public static async Task<QueryUserOrganisationApiResponse> PostQueryUserOrganisation(
        Guid userId,
        Guid organisationId,
        // ---
        [FromBody] QueryUserOrganisationApiRequestBody request,
        // ---
        IScopedSessionReader scopedSession,
        IInteractionDispatcher interaction,
        // ---
        CancellationToken cancellationToken = default)
    {
        var filteredOrganisationsResponse = await interaction.DispatchAsync(
            new FilterOrganisationsForUserRequest {
                ClientId = scopedSession.Application.ClientId,
                UserId = userId,
                Filter = request.Filter,
            }, cancellationToken
        ).To<FilterOrganisationsForUserResponse>();

        var organisation = filteredOrganisationsResponse.FilteredOrganisations
            .FirstOrDefault(organisation => organisation.Id == organisationId);

        return new() {
            UserId = userId,
            Organisation = organisation is null ? null : new OrganisationDetails {
                Id = organisation.Id,
                Name = organisation.Name,
                LegalName = organisation.LegalName
            }
        };
    }
}
