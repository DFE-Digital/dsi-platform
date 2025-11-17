namespace Dfe.SignIn.Core.Interfaces.Audit;

/// <summary>
/// Represents a service for building an <see cref="AuditContext"/> from the execution context.
/// </summary>
public interface IAuditContextBuilder
{
    /// <summary>
    /// Builds an <see cref="AuditContext"/> containing trace and source metadata.
    /// </summary>
    /// <returns>
    ///   <para>The constructed <see cref="AuditContext"/>.</para>
    /// </returns>
    AuditContext BuildAuditContext();
}
