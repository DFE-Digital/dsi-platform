using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class PpOrgAssoCacheEntityConfiguration : IEntityTypeConfiguration<PpOrgAssoCacheEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PpOrgAssoCacheEntity> builder)
    {
        builder.HasNoKey().ToTable("pp_org_asso_cache");

        builder.HasIndex(e => e.AssociatedMasterProviderCode, "idx_associatedMP");

        builder.Property(e => e.AssociatedMasterProviderCode)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("associatedMasterProviderCode");

        builder.Property(e => e.LinkType)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("linkType");

        builder.Property(e => e.MasterProviderCode)
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("masterProviderCode");
    }
}
