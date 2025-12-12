using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;

internal sealed class InvitationEntityConfiguration : IEntityTypeConfiguration<InvitationEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<InvitationEntity> builder)
    {
        builder.ToTable("invitation");

        builder.HasKey(e => e.Id)
            .HasName("PK_Invitation");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.ApproverEmail)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("approverEmail");

        builder.Property(e => e.Code)
            .HasMaxLength(15)
            .IsUnicode(false)
            .HasColumnName("code");

        builder.Property(e => e.CodeMetaData)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("codeMetaData");

        builder.Property(e => e.Completed)
            .HasColumnName("completed");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("createdAt");

        builder.Property(e => e.Deactivated)
            .HasColumnName("deactivated");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("email");

        builder.Property(e => e.FirstName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("firstName");

        builder.Property(e => e.IsApprover)
            .HasColumnName("isApprover");

        builder.Property(e => e.IsMigrated)
            .HasColumnName("isMigrated");

        builder.Property(e => e.LastName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("lastName");

        builder.Property(e => e.OrganisationName)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("orgName");

        builder.Property(e => e.OriginClientId)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("originClientId");

        builder.Property(e => e.OriginRedirectUri)
            .HasMaxLength(1024)
            .IsUnicode(false)
            .HasColumnName("originRedirectUri");

        builder.Property(e => e.OverrideBody)
            .HasColumnName("overrideBody");

        builder.Property(e => e.OverrideSubject)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("overrideSubject");

        builder.Property(e => e.PreviousPassword)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("previousPassword");

        builder.Property(e => e.PreviousSalt)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("previousSalt");

        builder.Property(e => e.PreviousUsername)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("previousUsername");

        builder.Property(e => e.Reason)
            .IsUnicode(false)
            .HasColumnName("reason");

        builder.Property(e => e.SelfStarted)
            .HasColumnName("selfStarted");

        builder.Property(e => e.Uid)
            .HasColumnName("uid");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updatedAt");
    }
}
