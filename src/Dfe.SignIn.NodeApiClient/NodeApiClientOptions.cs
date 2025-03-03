using Microsoft.Extensions.Options;

namespace Dfe.SignIn.NodeApiClient;

/// <summary>
/// Options for setting up Node API clients.
/// </summary>
public sealed class NodeApiClientOptions : IOptions<NodeApiClientOptions>
{
    /// <summary>
    /// Gets or sets the base address of the <see cref="NodeApiName.Access"/> API.
    /// </summary>
    public Uri? AccessBaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the base address of the <see cref="NodeApiName.Applications"/> API.
    /// </summary>
    public Uri? ApplicationsBaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the base address of the <see cref="NodeApiName.Directories"/> API.
    /// </summary>
    public Uri? DirectoriesBaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the base address of the <see cref="NodeApiName.Organisations"/> API.
    /// </summary>
    public Uri? OrganisationsBaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the base address of the <see cref="NodeApiName.Search"/> API.
    /// </summary>
    public Uri? SearchBaseAddress { get; set; }

    /// <inheritdoc/>
    NodeApiClientOptions IOptions<NodeApiClientOptions>.Value => this;
}
