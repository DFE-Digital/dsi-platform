using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class PasswordHistoryEntityConfiguration : IEntityTypeConfiguration<PasswordHistoryEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PasswordHistoryEntity> builder)
    {
        builder.ToTable("password_history");

        builder.HasKey(e => e.Id)
            .HasName("PK_user_password_history");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.Password)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("password");

        builder.Property(e => e.Salt)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("salt");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");
    }
}
