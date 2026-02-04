using Dfe.SignIn.InternalApi.Client;
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
    public IReadOnlyDictionary<string, InternalApiClientOptions> Apis { get; set; } = new Dictionary<string, InternalApiClientOptions>();

    /// <inheritdoc/>
    NodeApiClientOptions IOptions<NodeApiClientOptions>.Value => this;
}
