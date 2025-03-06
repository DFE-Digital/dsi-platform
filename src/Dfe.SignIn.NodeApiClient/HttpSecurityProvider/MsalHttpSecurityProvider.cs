
using Microsoft.Identity.Client;

namespace Dfe.SignIn.NodeApiClient.HttpSecurityProvider;

/// <summary>
/// MSAL implementation of IHttpSecurityProvider
/// </summary>
public sealed class MsalHttpSecurityProvider : IHttpSecurityProvider
{
    private readonly IConfidentialClientApplication confidentialClientApplication;
    private readonly string[] scopes;

    /// <summary>
    /// Create an instance of MsalHttpSecurityProvider
    /// </summary>
    /// <param name="scopes">Required scopes for AcquireTokenForClient</param>
    /// <param name="confidentialClientApplication">Instance of IConfidentialClientApplication</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="scopes"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="confidentialClientApplication"/> is null.</para>
    /// </exception>
    public MsalHttpSecurityProvider(string[] scopes, IConfidentialClientApplication confidentialClientApplication)
    {
        ArgumentNullException.ThrowIfNull(scopes, nameof(scopes));
        ArgumentNullException.ThrowIfNull(confidentialClientApplication, nameof(confidentialClientApplication));

        this.scopes = scopes;
        this.confidentialClientApplication = confidentialClientApplication;
    }

    /// <summary>
    /// Gets the AccessToken
    /// </summary>
    /// <returns></returns>
    private async Task<string?> GetAuthorizationBearerTokenAsync()
    {
        var authResult = await this.confidentialClientApplication
            .AcquireTokenForClient(scopes: this.scopes)
            .ExecuteAsync();

        return authResult.AccessToken;
    }

    /// <summary>
    /// Add authorization properties to the provided HttpRequestMessage
    /// </summary>
    /// <param name="httpRequestMessage">The HttpRequestMessage instance to add the bearer token to</param>
    /// <returns></returns>
    public async Task AddAuthorizationAsync(HttpRequestMessage httpRequestMessage)
    {
        string? bearerToken = await this.GetAuthorizationBearerTokenAsync();
        if (bearerToken is not null) {
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}