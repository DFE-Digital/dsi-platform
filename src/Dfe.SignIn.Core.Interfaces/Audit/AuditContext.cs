namespace Dfe.SignIn.Core.Interfaces.Audit;

/// <summary>
/// Contains metadata about the execution context of an audit event.
/// </summary>
public sealed record AuditContext
{
    /// <summary>
    /// The trace identifier for the current operation.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// The name of the environment that initiated the audit event.
    /// </summary>
    public required string EnvironmentName { get; init; }

    /// <summary>
    /// The name of the application that initiated the audit event.
    /// </summary>
    public required string SourceApplication { get; init; }

    /// <summary>
    /// The source IP address.
    /// </summary>
    public string? SourceIpAddress { get; init; }

    /// <summary>
    /// The ID of the user who initiated the audit event, if available.
    /// </summary>
    public Guid? SourceUserId { get; init; }
}
