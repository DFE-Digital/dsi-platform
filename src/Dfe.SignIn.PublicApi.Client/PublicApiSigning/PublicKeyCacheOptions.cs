using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.PublicApiSigning;

/// <summary>
/// Options for the DfE Sign-in public key cache.
/// </summary>
/// <remarks>
///   <para>The default options should be suitable for most services.</para>
/// </remarks>
public sealed class PublicKeyCacheOptions : IOptions<PublicKeyCacheOptions>
{
    /// <summary>
    /// Gets or sets the TTL of the cache.
    /// </summary>
    /// <remarks>
    ///   <para>By default ensures that cache is refreshed at least once every 24 hours.</para>
    /// </remarks>
    public TimeSpan TTL { get; set; } = new(hours: 24, minutes: 0, seconds: 0);

    /// <summary>
    /// Gets or sets the maximum refresh interval of the cache when new public
    /// key identifiers are encountered.
    /// </summary>
    /// <remarks>
    ///   <para>By default ensures that cache cannot be refreshed more than once every
    ///   10 minutes when a new public key identifier is encountered.</para>
    /// </remarks>
    public TimeSpan MaximumRefreshInterval { get; set; } = new(hours: 0, minutes: 10, seconds: 0);

    /// <inheritdoc/>
    PublicKeyCacheOptions IOptions<PublicKeyCacheOptions>.Value => this;
}
