using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

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
    public async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        GetOrganisationsAssociatedWithUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.OrganisationsAssociatedWithUserDto[]>(
            $"/organisations/v2/associated-with-user/{request.UserId}",
            cancellationToken
        );

        var organisations = response?.Select(org => mapper.Map<OrganisationModel>(org.Organisation)) ?? [];

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
