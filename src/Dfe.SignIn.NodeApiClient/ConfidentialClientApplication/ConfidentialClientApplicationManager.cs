using Microsoft.Identity.Client;

namespace Dfe.SignIn.NodeApiClient.ConfidentialClientApplication;

/// <summary>
/// An wrapper around ConfidentialClientApplicationBuilder for obtaining bearer tokens per NodeApiName.
/// </summary>
public sealed class ConfidentialClientApplicationManager : IConfidentialClientApplicationManager
{
    private readonly IReadOnlyDictionary<NodeApiName, IConfidentialClientApplication> confidentialClientApps;
    private readonly NodeApiAuthenticatedHttpClientOptions options;

    /// <summary>
    /// Create an instance of ConfidentialClientApplicationManager for the provided nodeApiNames.
    /// </summary>
    /// <param name="apis"></param>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="nodeApiNames"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="options"/> is null.</para>
    /// </exception>
    public ConfidentialClientApplicationManager(NodeApiName[] nodeApiNames, NodeApiAuthenticatedHttpClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(nodeApiNames, nameof(nodeApiNames));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        this.options = options;
        this.confidentialClientApps = nodeApiNames.ToDictionary(api => api, api => BuildConfidentialClientApp(this.options));
    }

    /// <summary>
    /// Build an instance of IConfidentialClientApplication with the given options.
    /// </summary>
    /// <param name="options">Options to configure the IConfidentialClientApplication instance.</param>
    /// <returns>The <see cref="IConfidentialClientApplication"/> instance.</returns>
    private static IConfidentialClientApplication BuildConfidentialClientApp(NodeApiAuthenticatedHttpClientOptions options)
    {
        return ConfidentialClientApplicationBuilder
            .Create(options.ClientId.ToString())
            .WithClientSecret(options.ClientSecret)
            .WithAuthority(options.HostUrl)
            .Build();
    }

    /// <summary>
    /// For the given NodeApiName attempt to get a BearerToken (AccessCode)
    /// </summary>
    /// <param name="nodeApiName"></param>
    /// <returns>
    /// <para>AccessToken</para>
    /// <para>- or -</para>
    /// <para>Null</para>
    // </returns>
    private async Task<string?> GetAuthorizationBearerTokenAsync(NodeApiName nodeApiName)
    {
        this.confidentialClientApps.TryGetValue(nodeApiName, out var confidentialClientApplication);

        if (confidentialClientApplication is null) {
            return null;
        }

        var authResult = await confidentialClientApplication
            .AcquireTokenForClient(scopes: [$"{this.options.Resource}/.default"])
            .ExecuteAsync();

        return authResult.AccessToken;
    }

    /// <summary>
    /// For the given httpRequestMessage augment it with the neccessary security requirements
    /// </summary>
    /// <param name="nodeApiName">NodeApiName determines which security requirements are applied</param>
    /// <param name="httpRequestMessage">The httpRequestMessage instance to apply security requirements to</param>
    /// <returns></returns>
    public async Task AddAuthorizationAsync(NodeApiName nodeApiName, HttpRequestMessage httpRequestMessage)
    {
        string? bearerToken = await this.GetAuthorizationBearerTokenAsync(nodeApiName);
        if (bearerToken is not null) {
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}