using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.GovNotify;

/// <summary>
/// Options for integration with the GOV Notify service.
/// </summary>
public sealed class GovNotifyOptions : IOptions<GovNotifyOptions>
{
    /// <summary>
    /// Gets the key that enables the application to interact with GOV Notify.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <inheritdoc/>
    GovNotifyOptions IOptions<GovNotifyOptions>.Value => this;
}
