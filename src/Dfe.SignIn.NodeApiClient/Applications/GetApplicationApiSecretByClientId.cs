
using System.Net.Http.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Applications;

/// <summary>
/// ApiRequester for obtaining an applications ApiSecret
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationApiSecretByClientId_ApiRequester : IInteractor<GetApplicationApiSecretByClientIdRequest, GetApplicationApiSecretByClientIdResponse>
{
    private readonly HttpClient httpClient;

    public GetApplicationApiSecretByClientId_ApiRequester([FromKeyedServices(NodeApiName.Applications)] HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<GetApplicationApiSecretByClientIdResponse> InvokeAsync(GetApplicationApiSecretByClientIdRequest request)
    {
        var response = await this.httpClient.GetFromJsonAsync<Models.ApplicationModel>($"services/{request.ClientId}");

        return new GetApplicationApiSecretByClientIdResponse {
            Application = response is null ? null : new Core.Models.Applications.ApplicationApiSecretModel {
                ClientId = response.RelyingParty.ClientId,
                ApiSecret = response.RelyingParty.ApiSecret
            }
        };
    }
}

