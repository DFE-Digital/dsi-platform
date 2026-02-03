using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserServiceRequestEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ServiceId { get; set; }

    public string? RoleIds { get; set; }

    public Guid OrganisationId { get; set; }

    public short Status { get; set; }

    public string? Reason { get; set; }

    public Guid? ActionedBy { get; set; }

    public string? ActionedReason { get; set; }

    public DateTime? ActionedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string RequestType { get; set; } = null!;

    public virtual OrganisationEntity Organisation { get; set; } = null!;
}
#pragma warning restore CS1591

