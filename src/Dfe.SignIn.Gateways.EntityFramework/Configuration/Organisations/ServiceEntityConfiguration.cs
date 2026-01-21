using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceEntityConfiguration : IEntityTypeConfiguration<ServiceEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceEntity> builder)
    {
        builder.ToTable("service");

        builder.HasKey(e => e.Id).HasName("PK__service__3213E83F63214D93");

        builder.HasIndex(e => e.ClientId, "UQ_service_clientid").IsUnique();

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.ApiSecret)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("apiSecret");

        builder.Property(e => e.ClientId)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("clientId");

        builder.Property(e => e.ClientSecret)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("clientSecret");

        builder.Property(e => e.Description)
            .IsUnicode(false)
            .HasColumnName("description");

        builder.Property(e => e.IsChildService).HasColumnName("isChildService");

        builder.Property(e => e.IsExternalService).HasColumnName("isExternalService");

        builder.Property(e => e.IsHiddenService).HasColumnName("isHiddenService");

        builder.Property(e => e.IsIdOnlyService).HasColumnName("isIdOnlyService");

        builder.Property(e => e.IsMigrated).HasColumnName("isMigrated");

        builder.Property(e => e.Name)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("name");

        builder.Property(e => e.ParentId).HasColumnName("parentId");

        builder.Property(e => e.PostResetUrl)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasColumnName("postResetUrl");

        builder.Property(e => e.ServiceHome)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasColumnName("serviceHome");

        builder.Property(e => e.TokenEndpointAuthMethod)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("tokenEndpointAuthMethod");

        builder.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName("FK_service_parent");
    }
}
