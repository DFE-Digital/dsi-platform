using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserServiceEntity
{
    public Guid Id { get; set; }

    public short Status { get; set; }

    public Guid UserId { get; set; }

    public Guid? OrganisationId { get; set; }

    public Guid? ServiceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastAccessed { get; set; }

    public virtual OrganisationEntity? Organisation { get; set; }

    public virtual ServiceEntity? Service { get; set; }
}
#pragma warning restore CS1591

