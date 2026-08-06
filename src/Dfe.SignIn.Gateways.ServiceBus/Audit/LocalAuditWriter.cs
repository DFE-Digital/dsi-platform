using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Gateways.ServiceBus.Audit;

/// <summary>
/// A local implementation of the IAuditWriter interface that logs audit messages locally.
/// </summary>
public sealed class LocalAuditWriter(
    ILogger<LocalAuditWriter> logger,
    IAuditContextBuilder contextAccessor) : AuditWriterBase(contextAccessor)
{
    /// <inheritdoc />
    protected override Task DispatchAuditAsync(string payload)
    {
        logger.LogInformation("Audit message: {Message}", payload);
        return Task.CompletedTask;
    }
}
