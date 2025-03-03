/*
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Applications;

[NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationByClientIdRequester
    : IApiRequester<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    private readonly HttpClient httpClient;

    public GetApplicationByClientIdRequester(
        [FromKeyedServices(NodeApiName.Access)] HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<GetApplicationByClientIdResponse> HandleAsync(GetApplicationByClientIdRequest request)
    {
        var httpResponse = await this.httpClient.GetAsync($"services/{request.ClientId}");
        throw new NotImplementedException();
    }
}
*/
