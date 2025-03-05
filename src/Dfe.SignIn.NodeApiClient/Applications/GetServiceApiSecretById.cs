
using System.Net.Http.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Applications;

/// <summary>
/// ApiRequester for obtaining a services ApiSecret
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetServiceApiSecretById_ApiRequester : IGetServiceApiSecretByServiceId
{
    private readonly HttpClient httpClient;

    public GetServiceApiSecretById_ApiRequester([FromKeyedServices(NodeApiName.Applications)] HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<GetServiceApiSecretByServiceIdResponse> InvokeAsync(GetServiceApiSecretByServiceIdRequest request)
    {
        var response = await this.httpClient.GetFromJsonAsync<Models.ServiceModel>($"services/{request.ServiceId}");

        //TODO: in the event that response is null what should be returned, or what exception thrown

        return new GetServiceApiSecretByServiceIdResponse {
            Service = new Core.Models.Applications.ServiceApiSecretModel {
                Id = response.Id,
                ApiSecret = response.RelyingParty.ApiSecret
            }
        };
    }
}

