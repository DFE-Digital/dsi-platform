using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// Handles writing audit events to Service Bus with contextual metadata and custom properties.
/// </summary>
/// <param name="contextAccessor">The audit context builder.</param>
/// <param name="sender">The Service Bus sender.</param>
public sealed class ServiceBusAuditWriter(IAuditContextBuilder contextAccessor,
    [FromKeyedServices(ServiceBusExtensions.AuditSenderKey)] ServiceBusSender sender) : AuditWriterBase(contextAccessor)
{
    /// <inheritdoc />
    protected override async Task DispatchAuditAsync(string payload)
    {
        var message = new ServiceBusMessage(payload);
        await sender.SendMessageAsync(message, CancellationToken.None);
    }
}
