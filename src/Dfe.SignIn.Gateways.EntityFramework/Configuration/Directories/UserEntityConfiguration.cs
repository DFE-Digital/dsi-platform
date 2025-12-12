using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("user");

        builder.HasKey(e => e.Sub)
            .HasName("PK__user__DDDF3AD9CA56D5BF");

        builder.HasIndex(e => e.EntraOid, "IDX__user__entra_oid__unique")
                .IsUnique()
                .HasFilter("([entra_oid] IS NOT NULL)");

        builder.Property(e => e.Sub)
                .ValueGeneratedNever()
                .HasColumnName("sub");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");

        builder.Property(e => e.EntraDeferUntil)
            .HasColumnType("datetime")
            .HasColumnName("entra_defer_until");

        builder.Property(e => e.EntraLinked)
            .HasColumnType("datetime")
            .HasColumnName("entra_linked");

        builder.Property(e => e.EntraOid)
            .HasColumnName("entra_oid");

        builder.Property(e => e.LastName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("family_name");

        builder.Property(e => e.FirstName)
            .HasMaxLength(255)
            .HasColumnName("given_name");

        builder.Property(e => e.IsEntra)
            .HasColumnName("is_entra");

        builder.Property(e => e.IsInternalUser)
            .HasColumnName("is_internal_user");

        builder.Property(e => e.IsMigrated)
            .HasColumnName("isMigrated");

        builder.Property<bool>("IsTestUser")
            .HasColumnName("is_test_user");

        builder.Property(e => e.JobTitle)
            .HasMaxLength(255)
            .HasColumnName("job_title");

        builder.Property(e => e.LastLogin)
            .HasColumnType("datetime")
            .HasColumnName("last_login");

        builder.Property(e => e.Password)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("password");

        builder.Property(e => e.PasswordResetRequired)
            .HasColumnName("password_reset_required");

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("phone_number");

        builder.Property(e => e.PrevLogin)
            .HasColumnType("datetime")
            .HasColumnName("prev_login");

        builder.Property(e => e.Salt)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("salt");

        builder.Property(e => e.Status)
            .HasColumnName("status");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");

    }
}
