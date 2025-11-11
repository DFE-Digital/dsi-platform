using System.Net;
using System.Net.Http.Headers;
using Azure.Core;
using Dfe.SignIn.Core.Interfaces.Audit;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Create a new DelegateHandler to implement a AuthenticatedHttpClientHandler
/// </summary>
public sealed class AuthenticatedHttpClientHandler(
    IAuditContextBuilder auditContextBuilder,
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

        var auditContext = auditContextBuilder.BuildAuditContext();
        if (!string.IsNullOrEmpty(auditContext.SourceApplication)) {
            request.Headers.Add(AuditHeaderNames.SourceApplicationName, auditContext.SourceApplication);
        }
        if (!string.IsNullOrEmpty(auditContext.SourceIpAddress)) {
            request.Headers.Add(AuditHeaderNames.SourceIpAddress, auditContext.SourceIpAddress);
        }
        if (auditContext.SourceUserId is not null) {
            request.Headers.Add(AuditHeaderNames.SourceUserId, auditContext.SourceUserId.ToString());
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden) {
            throw new HttpRequestException(HttpRequestError.ConnectionError);
        }

        return response;
    }
}
