using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class UserBannerEntityConfiguration : IEntityTypeConfiguration<UserBannerEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserBannerEntity> builder)
    {
        builder.ToTable("user_banners");

        builder.HasKey(e => e.Id).HasName("PK_user_banners_id");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.BannerData)
            .HasMaxLength(1000)
            .IsUnicode(false)
            .HasDefaultValueSql("(NULL)")
            .HasColumnName("bannerData");

        builder.Property(e => e.BannerId).HasColumnName("bannerId");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId).HasColumnName("userId");
    }
}
