using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class ToggleFlagEntityConfiguration : IEntityTypeConfiguration<ToggleFlagEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<ToggleFlagEntity> builder)
    {
        builder.HasNoKey();

        builder.HasIndex(e => new { e.Type, e.ServiceName }, "IXU_ToggleFlags_Type_ServiceName").IsUnique();

        builder.Property(e => e.ServiceName)
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.Type)
            .HasMaxLength(255)
            .IsUnicode(false);
    }
}
