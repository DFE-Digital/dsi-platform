using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class InvitationOrganisationEntityConfiguration : IEntityTypeConfiguration<InvitationOrganisationEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationOrganisationEntity> builder)
    {
        builder.ToTable("invitation_organisation");

        builder.HasKey(e => new { e.InvitationId, e.OrganisationId }).HasName("invitation_organisation_pk");

        builder.Property(e => e.InvitationId).HasColumnName("invitation_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.RoleId).HasColumnName("role_id");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(d => d.Organisation).WithMany(p => p.InvitationOrganisations)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("invitation_organisation_organisation_id_fk");
    }
}
