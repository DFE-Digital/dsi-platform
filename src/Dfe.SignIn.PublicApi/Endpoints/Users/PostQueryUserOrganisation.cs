using AutoMapper;
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
    public static async Task<QueryUserOrganisation_PublicApiResponse> PostQueryUserOrganisation(
        Guid userId,
        Guid organisationId,
        // ---
        [FromBody] QueryUserOrganisation_PublicApiRequestBody request,
        // ---
        IScopedSessionReader scopedSession,
        IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse> filterOrganisationsForUser,
        IMapper mapper,
        // ---
        CancellationToken cancellationToken = default)
    {
        var filteredOrganisationsResponse = await filterOrganisationsForUser.InvokeAsync(new() {
            ClientId = scopedSession.Application.ClientId,
            UserId = userId,
            Filter = request.Filter,
        }, cancellationToken);

        var organisation = filteredOrganisationsResponse.FilteredOrganisations
            .FirstOrDefault(organisation => organisation.Id == organisationId);

        return new() {
            UserId = userId,
            Organisation = mapper.Map<OrganisationDetails>(organisation),
        };
    }
}
