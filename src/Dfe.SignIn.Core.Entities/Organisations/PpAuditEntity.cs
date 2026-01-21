using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PpAuditEntity
{
    public int Id { get; set; }

    public int? Status { get; set; }

    public int? StatusStep1 { get; set; }

    public int? StatusStep2 { get; set; }

    public int? StatusStep3 { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? OrgCount { get; set; }

    public int? OrgAssoCount { get; set; }

    public string? MessageId { get; set; }
}
#pragma warning restore CS1591

