using Dfe.SignIn.NodeApiClient.ConfidentialClientApplication;

namespace Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;

/// <summary>
/// Create a new DelegateHandler to implement a AuthenticatedHttpClientHandler
/// </summary>
public sealed class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly NodeApiName nodeApiName;
    private readonly IConfidentialClientApplicationManager confidentialClientApplicationManager;

    /// <summary>
    /// Create a new instance of AuthenticatedHttpClientHandler with the provided options
    /// </summary>
    /// <param name="nodeApiName">NodeApiName for this instance.</param>
    /// <param name="confidentialClientApplicationManager">Confidential Client Application Manager instance.</param>
    public AuthenticatedHttpClientHandler(NodeApiName nodeApiName, IConfidentialClientApplicationManager confidentialClientApplicationManager)
    {
        this.nodeApiName = nodeApiName;
        this.confidentialClientApplicationManager = confidentialClientApplicationManager;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await this.confidentialClientApplicationManager.AddAuthorizationAsync(this.nodeApiName, request);
        return await base.SendAsync(request, cancellationToken);
    }
}