using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class UserCodeEntityConfiguration : IEntityTypeConfiguration<UserCodeEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserCodeEntity> builder)
    {
        builder.ToTable("user_code");

        builder.HasKey(e => new { e.Uid, e.CodeType })
            .HasName("PK__user_cod__E4DCEC89DE598738");

        builder.HasIndex(e => e.Email, "idx_user_code_email");

        builder.Property(e => e.Uid)
            .HasColumnName("uid");

        builder.Property(e => e.CodeType)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasDefaultValue("PasswordReset")
            .HasColumnName("codeType");

        builder.Property(e => e.ClientId)
            .HasMaxLength(255)
            .HasColumnName("clientId");

        builder.Property(e => e.Code)
            .HasMaxLength(255)
            .HasColumnName("code");

        builder.Property(e => e.ContextData)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("contextData");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("email");

        builder.Property(e => e.RedirectUri)
            .HasMaxLength(255)
            .HasColumnName("redirectUri");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");
    }
}
