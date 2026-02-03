using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class PpAuditEntityConfiguration : IEntityTypeConfiguration<PpAuditEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PpAuditEntity> builder)
    {
        builder.ToTable("pp_audit");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.EndDate)
            .HasDefaultValueSql("(getdate())")
            .HasColumnName("endDate");

        builder.Property(e => e.MessageId)
            .IsUnicode(false)
            .HasColumnName("messageId");

        builder.Property(e => e.OrgAssoCount).HasColumnName("orgAssoCount");

        builder.Property(e => e.OrgCount).HasColumnName("orgCount");

        builder.Property(e => e.StartDate)
            .HasDefaultValueSql("(getdate())")
            .HasColumnName("startDate");

        builder.Property(e => e.Status).HasColumnName("status");

        builder.Property(e => e.StatusStep1).HasColumnName("statusStep1");

        builder.Property(e => e.StatusStep2).HasColumnName("statusStep2");

        builder.Property(e => e.StatusStep3).HasColumnName("statusStep3");
    }
}
