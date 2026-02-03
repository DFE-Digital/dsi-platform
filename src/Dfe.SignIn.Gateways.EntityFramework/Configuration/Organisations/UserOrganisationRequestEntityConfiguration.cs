using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserOrganisationRequestEntityConfiguration : IEntityTypeConfiguration<UserOrganisationRequestEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserOrganisationRequestEntity> builder)
    {
        builder.ToTable("user_organisation_requests");

        builder.HasKey(e => e.Id).HasName("PK__user_org__3213E83F4F394CAA");

        builder.HasIndex(e => e.Status, "idx_user_organisation_requests_status");

        builder.HasIndex(e => new { e.UserId, e.Status }, "idx_user_organisation_requests_user_status");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.ActionedAt).HasColumnName("actioned_at");

        builder.Property(e => e.ActionedBy).HasColumnName("actioned_by");

        builder.Property(e => e.ActionedReason)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("actioned_reason");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.Reason)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("reason");

        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Organisation).WithMany(p => p.UserOrganisationRequests)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_organisation_requests_organisation_id_fk");
    }
}
