using System.Net.Http.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Users.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Access;

/// <summary>
/// ApiRequester for obtaining applications associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetApplicationByClientId_ApiRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient httpClient)
    : IInteractor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse>
{

    /// <inheritdoc/>
    public async Task<GetApplicationsAssociatedWithUserResponse> InvokeAsync(GetApplicationsAssociatedWithUserRequest request)
    {
        var response = await httpClient.GetFromJsonAsync<Models.ApplicationDto[]>($"users/{request.UserId}/services");

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
