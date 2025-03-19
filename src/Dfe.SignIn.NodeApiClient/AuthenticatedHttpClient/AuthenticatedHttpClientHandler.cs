using System.Net;
using Dfe.SignIn.NodeApiClient.HttpSecurityProvider;

namespace Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;

/// <summary>
/// Create a new DelegateHandler to implement a AuthenticatedHttpClientHandler
/// </summary>
public sealed class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IHttpSecurityProvider httpSecurityProvider;

    /// <summary>
    /// Instantiate an instance of AuthenticatedHttpClientHandler
    /// </summary>
    /// <param name="httpSecurityProvider"></param>
    public AuthenticatedHttpClientHandler(IHttpSecurityProvider httpSecurityProvider)
    {
        this.httpSecurityProvider = httpSecurityProvider;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await this.httpSecurityProvider.AddAuthorizationAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden) {
            throw new HttpRequestException(HttpRequestError.ConnectionError);
        }

        return response;
    }
}
