using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class UserPasswordPolicyEntityConfiguration : IEntityTypeConfiguration<UserPasswordPolicyEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserPasswordPolicyEntity> builder)
    {
        builder.ToTable("user_password_policy");

        builder.HasKey(e => e.Id)
            .HasName("PK_UserPasswordPolicy");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.PasswordHistoryLimit)
            .HasDefaultValue((short)3)
            .HasColumnName("password_history_limit");

        builder.Property(e => e.PolicyCode)
            .HasMaxLength(255)
            .HasColumnName("policyCode");

        builder.Property(e => e.Uid)
            .HasColumnName("uid");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");

        builder.HasOne(d => d.UidNavigation)
            .WithMany(p => p.UserPasswordPolicies)
            .HasForeignKey(d => d.Uid)
            .HasConstraintName("FK__user_passwo__uid__72E607DB");
    }
}
