using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Audit;

internal sealed class AuditLogMetaEntityConfiguration : IEntityTypeConfiguration<AuditLogMetaEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<AuditLogMetaEntity> builder)
    {
        builder.ToTable("AuditLogMeta");

        builder.HasKey(e => e.Id).HasName("PK__AuditLog__3213E83F55A04F09");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.AuditId).HasColumnName("auditId");

        builder.Property(e => e.Key)
            .HasMaxLength(125)
            .HasColumnName("key");

        builder.Property(e => e.Value).HasColumnName("value");

        builder.HasOne(d => d.Audit).WithMany(p => p.AuditLogMeta)
            .HasForeignKey(d => d.AuditId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("AuditLog_FK");
    }
}
