using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.Interfaces.Audit;

/// <summary>
/// Configuration options for auditing features.
/// </summary>
public sealed class AuditOptions : IOptions<AuditOptions>
{
    /// <summary>
    /// The name of the application generating the audit event.
    /// </summary>
    public required string ApplicationName { get; set; }

    /// <summary>
    /// The name of the application environment.
    /// </summary>
    public string EnvironmentName { get; set; } = "local";

    /// <inheritdoc/>
    AuditOptions IOptions<AuditOptions>.Value => this;
}
