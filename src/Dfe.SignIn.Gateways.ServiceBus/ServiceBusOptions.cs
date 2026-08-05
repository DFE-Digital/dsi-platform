using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Options for configuring Service Bus.
/// </summary>
public sealed class ServiceBusOptions : IOptions<ServiceBusOptions>
{
    /// <summary>
    /// Gets or sets the Service Bus namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the Service Bus connection string (used for local emulator / development).
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <inheritdoc/>
    ServiceBusOptions IOptions<ServiceBusOptions>.Value => this;
}
