using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class InvitationServiceRoleEntity
{
    public Guid Id { get; set; }

    public Guid InvitationId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual RoleEntity Role { get; set; } = null!;
}
#pragma warning restore CS1591

