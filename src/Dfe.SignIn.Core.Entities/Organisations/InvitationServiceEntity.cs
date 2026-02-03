using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class InvitationServiceEntity
{
    public Guid InvitationId { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid ServiceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual OrganisationEntity Organisation { get; set; } = null!;

    public virtual ServiceEntity Service { get; set; } = null!;
}
#pragma warning restore CS1591

