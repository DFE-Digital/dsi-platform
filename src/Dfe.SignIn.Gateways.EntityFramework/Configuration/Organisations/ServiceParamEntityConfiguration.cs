using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ServiceParamEntityConfiguration : IEntityTypeConfiguration<ServiceParamEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ServiceParamEntity> builder)
    {

        builder.ToTable("serviceParams");

        builder.HasKey(e => new { e.ServiceId, e.ParamName });

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.ParamName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("paramName");

        builder.Property(e => e.ParamValue)
            .IsUnicode(false)
            .HasColumnName("paramValue");

        builder.HasOne(d => d.Service).WithMany(p => p.ServiceParams)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_serviceParams_service");
    }
}
