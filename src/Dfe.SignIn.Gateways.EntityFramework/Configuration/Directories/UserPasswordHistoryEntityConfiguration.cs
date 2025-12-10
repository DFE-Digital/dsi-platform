using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class UserPasswordHistoryEntityConfiguration : IEntityTypeConfiguration<UserPasswordHistoryEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserPasswordHistoryEntity> builder)
    {
        builder.ToTable("user_password_history");

        builder.HasKey(e => new { e.PasswordHistoryId, e.UserSub })
            .HasName("ck_user_password_history");

        builder.Property(e => e.PasswordHistoryId)
            .HasColumnName("passwordHistoryId");

        builder.Property(e => e.UserSub)
            .HasColumnName("userSub");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");
    }
}
