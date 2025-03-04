/*
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Applications;

[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationByClientId_ApiRequester : IGetApplicationByClientIdInteractor
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
