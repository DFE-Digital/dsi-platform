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
    public required string Namespace { get; set; }

    /// <inheritdoc/>
    ServiceBusOptions IOptions<ServiceBusOptions>.Value => this;
}
