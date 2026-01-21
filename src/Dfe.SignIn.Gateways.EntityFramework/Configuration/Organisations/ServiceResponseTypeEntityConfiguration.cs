using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceResponseTypeEntityConfiguration : IEntityTypeConfiguration<ServiceResponseTypeEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceResponseTypeEntity> builder)
    {
        builder.ToTable("serviceResponseTypes");

        builder.HasKey(e => new { e.ServiceId, e.ResponseType }).IsClustered(false);

        builder.HasIndex(e => e.ServiceId, "IX_serviceResponseTypes_serviceId").IsClustered();

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.ResponseType)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("responseType");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceResponseTypes)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceResponseTypes_service");
    }
}
