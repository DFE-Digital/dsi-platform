using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceRedirectUriEntityConfiguration : IEntityTypeConfiguration<ServiceRedirectUriEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceRedirectUriEntity> builder)
    {
        builder.ToTable("serviceRedirectUris");

        builder.HasKey(e => new { e.ServiceId, e.RedirectUrl }).IsClustered(false);

        builder.HasIndex(e => e.ServiceId, "IX_serviceRedirectUris_serviceId").IsClustered();

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.RedirectUrl)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasColumnName("redirectUrl");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceRedirectUris)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceRedirectUris_service");
    }
}
