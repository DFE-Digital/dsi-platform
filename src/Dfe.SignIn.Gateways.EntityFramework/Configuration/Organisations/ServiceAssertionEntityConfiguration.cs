using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceAssertionEntityConfiguration : IEntityTypeConfiguration<ServiceAssertionEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceAssertionEntity> builder)
    {
        builder.ToTable("serviceAssertions");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.FriendlyName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("friendlyName");

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.TypeUrn)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("typeUrn");

        builder.Property(e => e.Value)
            .HasMaxLength(512)
            .IsUnicode(false)
            .HasColumnName("value");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceAssertions)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceAssertions_service");
    }
}
