using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Audit;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class AuditLogEntity
{
    public Guid Id { get; set; }

    public string? Level { get; set; }

    public string? Message { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public string? Application { get; set; }

    public string? Environment { get; set; }

    public string? Type { get; set; }

    public string? SubType { get; set; }

    public Guid? UserId { get; set; }

    public Guid? OrganisationId { get; set; }

    public string? RequestIp { get; set; }

    public virtual ICollection<AuditLogMetaEntity> AuditLogMeta { get; set; } = [];
}
#pragma warning restore CS1591

