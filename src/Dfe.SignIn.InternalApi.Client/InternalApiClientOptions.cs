using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Options for setting up an internal API client.
/// </summary>
public sealed class InternalApiClientOptions : IOptions<InternalApiClientOptions>
{
    /// <summary>
    /// Gets or sets the base address of the API.
    /// </summary>
    public required Uri BaseAddress { get; set; }

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

    /// <inheritdoc/>
    InternalApiClientOptions IOptions<InternalApiClientOptions>.Value => this;
}
