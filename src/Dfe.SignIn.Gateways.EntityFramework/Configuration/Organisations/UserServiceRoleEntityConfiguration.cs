using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserServiceRoleEntityConfiguration : IEntityTypeConfiguration<UserServiceRoleEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserServiceRoleEntity> builder)
    {
        builder.ToTable("user_service_roles");

        builder.HasKey(e => e.Id).HasName("PK_UserServiceRoles");

        builder.HasIndex(e => new { e.UserId, e.RoleId, e.ServiceId, e.OrganisationId }, "UNQ_role_service_organisation").IsUnique();

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.RoleId).HasColumnName("role_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Role).WithMany(p => p.UserServiceRoles)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserServiceRoles_Role");
    }
}
