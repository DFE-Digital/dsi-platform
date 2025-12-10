using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class UserStatusChangeReasonEntityConfiguration : IEntityTypeConfiguration<UserStatusChangeReasonEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<UserStatusChangeReasonEntity> builder)
    {
        builder.ToTable("user_status_change_reasons");

        builder.HasKey(e => new { e.Id, e.UserId })
            .HasName("PK_UserStatusChangeReasons");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.NewStatus)
            .HasColumnName("new_status");

        builder.Property(e => e.OldStatus)
            .HasColumnName("old_status");

        builder.Property(e => e.Reason)
            .HasMaxLength(5000)
            .IsUnicode(false)
            .HasColumnName("reason");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");

        builder.HasOne(d => d.User)
            .WithMany(p => p.UserStatusChangeReasons)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserStatusChangeReasons_User");
    }
}
