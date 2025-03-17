using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.Users.Interactions;
using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
/// <param name="httpClient"></param>
/// <param name="mapper"></param>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationsAssociatedWithUser_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient, IMapper mapper)
    : IInteractor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{

    /// <inheritdoc/>
    public async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(GetOrganisationsAssociatedWithUserRequest request)
    {
        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.OrganisationsAssociatedWithUserDto[]>($"/organisations/v2/associated-with-user/{request.UserId}");

        var organisations = response?.Select(org => mapper.Map<OrganisationModel>(org.Organisation)) ?? [];

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
