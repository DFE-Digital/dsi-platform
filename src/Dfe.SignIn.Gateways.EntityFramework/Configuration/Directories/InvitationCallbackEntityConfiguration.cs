using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class InvitationCallbackEntityConfiguration : IEntityTypeConfiguration<InvitationCallbackEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationCallbackEntity> builder)
    {
        builder.ToTable("invitation_callback");

        builder.HasKey(e => new { e.InvitationId, e.SourceId })
            .HasName("PK_InvitationCallback");

        builder.Property(e => e.InvitationId)
            .HasColumnName("invitationId");

        builder.Property(e => e.SourceId)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("sourceId");

        builder.Property(e => e.CallbackUrl)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasColumnName("callbackUrl");

        builder.Property(e => e.ClientId)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("clientId");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");

        builder.HasOne(d => d.Invitation).WithMany(p => p.InvitationCallbacks)
            .HasForeignKey(d => d.InvitationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_InvitationCallback_Invitation");
    }
}
