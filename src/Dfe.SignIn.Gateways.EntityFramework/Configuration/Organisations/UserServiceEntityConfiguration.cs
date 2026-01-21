using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserServiceEntityConfiguration : IEntityTypeConfiguration<UserServiceEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserServiceEntity> builder)
    {
        builder.ToTable("user_services");

        builder.HasKey(e => e.Id).HasName("PK__user_ser__3213E83F28EA2180");

        builder.HasIndex(e => new { e.UserId, e.OrganisationId }, "IX_UserServices_UserOrg");

        builder.HasIndex(e => new { e.OrganisationId, e.ServiceId }, "idx_user_services_org_service_include_status");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.LastAccessed)
            .HasDefaultValueSql("(NULL)")
            .HasColumnType("datetime")
            .HasColumnName("lastAccessed");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.ServiceId).HasColumnName("service_id");

        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.HasOne(d => d.Organisation).WithMany(p => p.UserServices)
            .HasForeignKey(d => d.OrganisationId)
            .HasConstraintName("user_services_organisation_id_fk");

        builder.HasOne(d => d.Service).WithMany(p => p.UserServices)
            .HasForeignKey(d => d.ServiceId)
            .HasConstraintName("user_services_service_id_fk");
    }
}
