using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PolicyEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid ApplicationId { get; set; }

    public short Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PolicyConditionEntity> PolicyConditions { get; set; } = [];

    public virtual ICollection<PolicyRoleEntity> PolicyRoles { get; set; } = [];
}
#pragma warning restore CS1591

