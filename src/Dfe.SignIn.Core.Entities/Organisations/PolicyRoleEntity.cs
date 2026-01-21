using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PolicyRoleEntity
{
    public Guid PolicyId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PolicyEntity Policy { get; set; } = null!;

    public virtual RoleEntity Role { get; set; } = null!;
}
#pragma warning restore CS1591

