using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceBannerEntityConfiguration : IEntityTypeConfiguration<ServiceBannerEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceBannerEntity> builder)
    {
        builder.ToTable("serviceBanners");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.IsActive).HasColumnName("isActive");

        builder.Property(e => e.Message)
            .HasColumnName("message");

        builder.Property(e => e.Name)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("name");

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.Title)
            .IsUnicode(false)
            .HasColumnName("title");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.ValidFrom).HasColumnName("validFrom");

        builder.Property(e => e.ValidTo).HasColumnName("validTo");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceBanners)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceBanners");
    }
}
