using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class InvitationServiceRoleEntityConfiguration : IEntityTypeConfiguration<InvitationServiceRoleEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationServiceRoleEntity> builder)
    {
        builder.ToTable("invitation_service_roles");

        builder.HasKey(e => e.Id).HasName("PK_InvitationServiceRoles");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.InvitationId).HasColumnName("invitation_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.RoleId).HasColumnName("role_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.HasOne(d => d.Role).WithMany(p => p.InvitationServiceRoles)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_InvitationServiceRoles_Role");
    }
}
