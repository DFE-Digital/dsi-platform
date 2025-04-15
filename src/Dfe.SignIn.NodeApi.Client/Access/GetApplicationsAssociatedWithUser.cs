using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Users;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for obtaining applications associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetApplicationsAssociatedWithUser_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient httpClient)
    : IInteractor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse>
{

    /// <inheritdoc/>
    public async Task<GetApplicationsAssociatedWithUserResponse> InvokeAsync(
        GetApplicationsAssociatedWithUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.ApplicationDto[]>(
            $"users/{request.UserId}/services",
            cancellationToken
        );

        return new GetApplicationsAssociatedWithUserResponse {
            UserApplicationMappings = response?.Select(s => new UserApplicationMappingModel {
                UserId = s.UserId,
                AccessGranted = s.AccessGrantedOn,
                OrganisationId = s.OrganisationId,
                ApplicationId = s.ServiceId
            }) ?? []
        };
    }
}
