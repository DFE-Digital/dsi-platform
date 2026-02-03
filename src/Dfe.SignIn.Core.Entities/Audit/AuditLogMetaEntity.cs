using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Audit;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class AuditLogMetaEntity
{
    public Guid Id { get; set; }

    public Guid AuditId { get; set; }

    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public virtual AuditLogEntity Audit { get; set; } = null!;
}
#pragma warning restore CS1591

