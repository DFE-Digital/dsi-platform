using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class PolicyRoleEntityConfiguration : IEntityTypeConfiguration<PolicyRoleEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PolicyRoleEntity> builder)
    {
        builder.ToTable("PolicyRole");

        builder.HasKey(e => new { e.PolicyId, e.RoleId });

        builder.HasOne(d => d.Policy).WithMany(p => p.PolicyRoles)
            .HasForeignKey(d => d.PolicyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_PolicyRole_Policy");

        builder.HasOne(d => d.Role).WithMany(p => p.PolicyRoles)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_PolicyRole_Role");
    }
}
