using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class InvitationServiceIdentifierEntityConfiguration : IEntityTypeConfiguration<InvitationServiceIdentifierEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationServiceIdentifierEntity> builder)
    {
        builder.ToTable("invitation_service_identifiers");

        builder.HasKey(e => new { e.InvitationId, e.ServiceId, e.OrganisationId, e.IdentifierKey }).HasName("invitation_service_identifiers_pkey");

        builder.Property(e => e.InvitationId).HasColumnName("invitation_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.IdentifierKey)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("identifier_key");

        builder.Property(e => e.IdentifierValue)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("identifier_value");
    }
}
