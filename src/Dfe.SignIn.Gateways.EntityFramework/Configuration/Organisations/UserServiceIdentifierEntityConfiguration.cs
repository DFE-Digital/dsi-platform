using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserServiceIdentifierEntityConfiguration : IEntityTypeConfiguration<UserServiceIdentifierEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserServiceIdentifierEntity> builder)
    {
        builder.ToTable("user_service_identifiers");

        builder.HasKey(e => new { e.UserId, e.ServiceId, e.OrganisationId, e.IdentifierKey }).HasName("user_service_identifiers_pkey");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.IdentifierKey)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("identifier_key");

        builder.Property(e => e.IdentifierValue).HasColumnName("identifier_value");
    }
}
