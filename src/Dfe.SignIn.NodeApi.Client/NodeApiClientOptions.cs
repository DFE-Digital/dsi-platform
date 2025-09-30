using Microsoft.Extensions.Options;

namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// Options for setting up Node API clients.
/// </summary>
public sealed class NodeApiClientOptions : IOptions<NodeApiClientOptions>
{
    /// <summary>
    /// Gets or sets per-API options.
    /// </summary>
    public IReadOnlyDictionary<string, NodeApiOptions> Apis { get; set; } = new Dictionary<string, NodeApiOptions>();

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

    /// <inheritdoc/>
    NodeApiClientOptions IOptions<NodeApiClientOptions>.Value => this;
}

/// <summary>
/// Options for a specific mid-tier API.
/// </summary>
public sealed class NodeApiOptions
{
    /// <summary>
    /// Gets or sets the base address of the API.
    /// </summary>
    public required Uri BaseAddress { get; set; }
}
