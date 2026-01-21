using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceGrantTypeEntityConfiguration : IEntityTypeConfiguration<ServiceGrantTypeEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceGrantTypeEntity> builder)
    {
        builder.ToTable("serviceGrantTypes");

        builder.HasKey(e => new { e.ServiceId, e.GrantType }).IsClustered(false);

        builder.HasIndex(e => e.ServiceId, "IX_serviceGrantTypes_serviceId").IsClustered();

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.GrantType)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("grantType");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceGrantTypes)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceGrantTypes_service");
    }
}
