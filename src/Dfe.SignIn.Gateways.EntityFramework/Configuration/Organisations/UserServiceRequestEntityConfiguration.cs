using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserServiceRequestEntityConfiguration : IEntityTypeConfiguration<UserServiceRequestEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserServiceRequestEntity> builder)
    {
        builder.ToTable("user_service_requests");

        builder.HasKey(e => e.Id).HasName("PK_UserServiceRerquests");

        builder.HasIndex(e => new { e.OrganisationId, e.Status }, "idx_user_service_requests_organisation_status");

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

        builder.Property(e => e.RequestType)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasDefaultValue("service")
            .HasColumnName("request_type");

        builder.Property(e => e.RoleIds)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("role_ids");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Organisation).WithMany(p => p.UserServiceRequests)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("user_service_requests_organisation_id_fk");
    }
}
