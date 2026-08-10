namespace Dfe.SignIn.Core.Contracts.Audit;

/// <summary>
/// An interface representing an audit writer
/// </summary>
public interface IAuditWriter
{
    /// <summary>
    /// Implementation of an audit logger.
    /// </summary>
    /// <param name="auditRequest">The audit request to log.</param>
    /// <returns>The response from the audit log operation.</returns>
    Task Log(WriteToAuditRequest auditRequest);
}
