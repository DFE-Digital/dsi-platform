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
