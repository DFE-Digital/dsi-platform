using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class TokenEntityConfiguration : IEntityTypeConfiguration<TokenEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<TokenEntity> builder)
    {
        builder.ToTable("tokens");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.Active).HasColumnName("active");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.Exp).HasColumnName("exp");

        builder.Property(e => e.GrantId).HasColumnName("grantId");

        builder.Property(e => e.Jti)
            .IsUnicode(false)
            .HasColumnName("jti");

        builder.Property(e => e.Kind)
            .IsUnicode(false)
            .HasColumnName("kind");

        builder.Property(e => e.Sid).HasColumnName("sid");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(d => d.Grant).WithMany(p => p.Tokens)
            .HasForeignKey(d => d.GrantId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_tokens");
    }
}
