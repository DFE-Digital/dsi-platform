using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class OrganisationAssociationEntityConfiguration : IEntityTypeConfiguration<OrganisationAssociationEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<OrganisationAssociationEntity> builder)
    {
        builder.HasNoKey().ToTable("organisation_association");

        builder.HasIndex(e => e.OrganisationId, "IX_OrganisationAssociation_OrgAssType");

        builder.Property(e => e.AssociatedOrganisationId).HasColumnName("associated_organisation_id");

        builder.Property(e => e.LinkType)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("link_type");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.HasOne(d => d.AssociatedOrganisation).WithMany()
            .HasForeignKey(d => d.AssociatedOrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__organisat__assoc__37703C52");

        builder.HasOne(d => d.Organisation).WithMany()
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__organisat__organ__3864608B");
    }
}
