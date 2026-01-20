using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Audit;

internal sealed class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(e => e.Id).HasName("PK__AuditLog__3213E83FDC38F9AB");

        builder.HasIndex(e => e.CreatedAt, "createdAt").IsDescending();

        builder.HasIndex(e => e.Level, "level");

        builder.HasIndex(e => e.Type, "type");

        builder.HasIndex(e => e.UserId, "userId");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.Application)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("application");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.Environment)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("environment");

        builder.Property(e => e.Level)
            .HasMaxLength(255)
            .HasColumnName("level");

        builder.Property(e => e.Message).HasColumnName("message");

        builder.Property(e => e.OrganisationId).HasColumnName("organisationid");

        builder.Property(e => e.RequestIp)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("requestIp");

        builder.Property(e => e.SubType)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("subType");

        builder.Property(e => e.Type)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("type");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId).HasColumnName("userId");
    }
}
