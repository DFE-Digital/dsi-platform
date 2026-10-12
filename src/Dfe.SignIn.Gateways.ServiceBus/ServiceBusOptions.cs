using Dfe.SignIn.WebFramework.Extensions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.ServiceBus;

/// <summary>
/// Options for configuring Service Bus.
/// </summary>
public sealed class ServiceBusOptions : IApplicationSettings, IOptions<ServiceBusOptions>
{
    /// <inheritdoc/>
    public static string SectionName => "ServiceBus";

    /// <summary>
    /// Gets or sets the Service Bus namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <inheritdoc/>
    ServiceBusOptions IOptions<ServiceBusOptions>.Value => this;
}
