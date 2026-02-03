using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserOrganisationEntity
{
    public Guid UserId { get; set; }

    public Guid OrganisationId { get; set; }

    public short RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public short Status { get; set; }

    public string? Reason { get; set; }

    public long? NumericIdentifier { get; set; }

    public string? TextIdentifier { get; set; }

    public virtual OrganisationEntity Organisation { get; set; } = null!;
}
#pragma warning restore CS1591

