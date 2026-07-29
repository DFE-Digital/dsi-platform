using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Audit;

/// <summary>
/// An interface representing an audit writer
/// </summary>
public interface IAuditWriter
{
    /// <summary>
    /// Implementation of an audit logger.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task<WriteToAuditResponse> Log(
       InteractionContext<WriteToAuditRequest> context);
}
