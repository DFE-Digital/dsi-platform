using System.Net;
using System.Net.Http.Headers;
using Azure.Core;

namespace Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;

/// <summary>
/// Create a new DelegateHandler to implement a AuthenticatedHttpClientHandler
/// </summary>
public sealed class AuthenticatedHttpClientHandler(
    TokenCredential credential,
    string[] scopes
) : DelegatingHandler
{
    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await credential.GetTokenAsync(new(scopes), cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden) {
            throw new HttpRequestException(HttpRequestError.ConnectionError);
        }

        return response;
    }
}
