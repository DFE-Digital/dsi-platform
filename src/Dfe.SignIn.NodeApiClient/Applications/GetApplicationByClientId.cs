using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Applications;

/// <summary>
/// ApiRequester for obtaining an application.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationByClientId_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Applications)] HttpClient httpClient
) : IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    /// <inheritdoc/>
    public async Task<GetApplicationByClientIdResponse> InvokeAsync(GetApplicationByClientIdRequest request)
    {
        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.ApplicationModelDto>($"services/{request.ClientId}");

        return new GetApplicationByClientIdResponse {
            Application = response is null ? null : new() {
                ApiSecret = response.RelyingParty.ApiSecret,
                ClientId = response.RelyingParty.ClientId,
                Description = response.Description,
                Id = response.Id,
                Name = response.Name,
                ServiceHomeUrl = new Uri(response.RelyingParty.ServiceHome),
                IsExternalService = response.IsExternalService,
                IsHiddenService = response.IsHiddenService,
                IsIdOnlyService = response.IsIdOnlyService
            }
        };
    }
}
