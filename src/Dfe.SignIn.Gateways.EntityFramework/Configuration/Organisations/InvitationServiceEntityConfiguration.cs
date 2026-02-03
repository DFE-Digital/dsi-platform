using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class InvitationServiceEntityConfiguration : IEntityTypeConfiguration<InvitationServiceEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationServiceEntity> builder)
    {
        builder.ToTable("invitation_services");

        builder.HasKey(e => new { e.InvitationId, e.OrganisationId, e.ServiceId }).HasName("PK_InvitationServices");

        builder.Property(e => e.InvitationId).HasColumnName("invitation_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(d => d.Organisation).WithMany(p => p.InvitationServices)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("invitation_services_organisation_id_fk");

        builder.HasOne(d => d.Service).WithMany(p => p.InvitationServices)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("invitation_services_service_id_fk");
    }
}
