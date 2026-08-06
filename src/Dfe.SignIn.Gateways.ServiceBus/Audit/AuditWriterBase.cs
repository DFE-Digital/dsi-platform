using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// Abstract base class for audit writers that provides common functionality for logging audit events.
/// </summary>
/// <param name="contextAccessor">The audit context builder.</param>
public abstract class AuditWriterBase(IAuditContextBuilder contextAccessor) : IAuditWriter
{
    /// <summary>
    /// Logs an audit event by serializing the request and dispatching it to the appropriate destination.
    /// </summary>
    /// <param name="request">The audit request to log.</param>
    /// <returns>The response from the audit logging operation.</returns>
    public async Task<WriteToAuditResponse> Log(WriteToAuditRequest request)
    {
        var auditContext = contextAccessor.BuildAuditContext();
        string json = AuditSerializer.SerializeMessageBody(auditContext, request);

        await this.DispatchAuditAsync(json);

        return new WriteToAuditResponse();
    }

    /// <summary>
    /// Dispatches the serialized audit message to the appropriate destination (e.g., Service Bus, local log).
    /// </summary>
    /// <param name="payload">The serialized audit message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task DispatchAuditAsync(string payload);
}
