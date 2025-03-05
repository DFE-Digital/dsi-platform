using Microsoft.Extensions.Options;

namespace Dfe.SignIn.NodeApiClient;

/// <summary>
/// Options for setting up Node API clients.
/// </summary>
public sealed class NodeApiClientOptions : IOptions<NodeApiClientOptions>
{
    /// <summary>
    /// Gets or sets per-API options.
    /// </summary>
    public IEnumerable<NodeApiOptions> Apis { get; set; } = [];

    /// <summary>
    /// Gets or sets common Authenticated Http Client Options
    /// </summary>
    public NodeApiAuthenticatedHttpClientOptions AuthenticatedHttpClientOptions { get; set; } = new();

    /// <inheritdoc/>
    NodeApiClientOptions IOptions<NodeApiClientOptions>.Value => this;
}

/// <summary>
/// Options for a specific mid-tier API.
/// </summary>
public sealed class NodeApiOptions
{
    /// <summary>
    /// Gets or sets the name of the API.
    /// </summary>
    public required NodeApiName ApiName { get; set; }

    /// <summary>
    /// Gets or sets the base address of the API.
    /// </summary>
    public required Uri BaseAddress { get; set; }
}

public sealed class NodeApiAuthenticatedHttpClientOptions
{
    /// <summary>
    /// Client ID (also known as App ID) of the application as registered in the
    /// application registration portal (https://aka.ms/msal-net-register-app)/.
    /// </summary>
    public Guid ClientId { get; set; } = Guid.Empty;

    /// <summary>
    /// Tenant value as defined in Azure
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// Secret string previously shared with AAD at application registration to prove the identity
    /// of the application (the client) requesting the tokens
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// URI of the authority from which MSAL.NET will acquire the tokens.
    /// </summary>
    public Uri HostUrl { get; set; } = default!;

    /// <summary>
    /// Resource value as defined within Azure.
    /// </summary>
    public Guid Resource { get; set; } = Guid.Empty;

    /// <summary>
    /// Url of internal proxy address
    /// </summary>
    public Uri ProxyUrl { get; set; } = default!;

    /// <summary>
    /// Indicates whether the proxy should be used
    /// </summary>
    public bool UseProxy { get; set; } = false;
}