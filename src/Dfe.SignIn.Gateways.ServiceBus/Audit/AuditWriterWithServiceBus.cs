using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// Handles writing audit events to Service Bus with contextual metadata and custom properties.
/// </summary>
/// <param name="contextAccessor"></param>
/// <param name="sender"></param>
public sealed class AuditWriterWithServiceBus(IAuditContextBuilder contextAccessor,
    [FromKeyedServices(ServiceBusExtensions.AuditSenderKey)] ServiceBusSender sender) : IAuditWriter
{
    /// <summary>
    /// Implementation of an audit logger.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<WriteToAuditResponse> Log(WriteToAuditRequest request)
    {
        var auditContext = contextAccessor.BuildAuditContext();

        string json = AuditSerializer.SerializeMessageBody(auditContext, request);

        var message = new ServiceBusMessage(json);
        await sender.SendMessageAsync(message, CancellationToken.None);

        return new WriteToAuditResponse();
    }
}

/// <summary>
/// A null implementation of the IAuditWriter interface that does not perform any logging.
/// </summary>
public sealed class NullAuditWriter : IAuditWriter
{
    /// <summary>
    /// Implementation of an audit logger.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<WriteToAuditResponse> Log(WriteToAuditRequest request)
    {
        return await Task.FromResult(new WriteToAuditResponse());
    }
}
