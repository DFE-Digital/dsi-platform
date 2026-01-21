using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserOrganisationEntityConfiguration : IEntityTypeConfiguration<UserOrganisationEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserOrganisationEntity> builder)
    {
        builder.ToTable("user_organisation");

        builder.HasKey(e => new { e.UserId, e.OrganisationId }).HasName("user_organisation_pk");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.NumericIdentifier).HasColumnName("numeric_identifier");

        builder.Property(e => e.Reason)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("reason");

        builder.Property(e => e.RoleId).HasColumnName("role_id");

        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.TextIdentifier)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("text_identifier");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(d => d.Organisation).WithMany(p => p.UserOrganisations)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_organisation_organisation_id_fk");
    }
}
