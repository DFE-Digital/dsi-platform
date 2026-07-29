using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// Handles writing audit events to Service Bus with contextual metadata and custom properties.
/// </summary>
[Obsolete("This class will be removed in future versions. Use an instance of IAuditWriter")]
public sealed class WriteToAuditWithServiceBus(
    IAuditContextBuilder contextAccessor,
    [FromKeyedServices(ServiceBusExtensions.AuditSenderKey)] ServiceBusSender sender
) : Interactor<WriteToAuditRequest, WriteToAuditResponse>
{

    /// <inheritdoc/>
    public override async Task<WriteToAuditResponse> InvokeAsync(
        InteractionContext<WriteToAuditRequest> context,
        CancellationToken cancellationToken = default)
    {
        var auditContext = contextAccessor.BuildAuditContext();

        string json = AuditSerializer.SerializeMessageBody(auditContext, context.Request);

        var message = new ServiceBusMessage(json);
        await sender.SendMessageAsync(message, CancellationToken.None);

        return new WriteToAuditResponse();
    }
}
