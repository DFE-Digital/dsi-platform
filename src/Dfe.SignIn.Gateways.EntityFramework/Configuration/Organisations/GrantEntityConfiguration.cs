using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class GrantEntityConfiguration : IEntityTypeConfiguration<GrantEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<GrantEntity> builder)
    {
        builder.ToTable("grants");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.Email)
            .IsUnicode(false)
            .HasColumnName("email");

        builder.Property(e => e.Jti)
            .IsUnicode(false)
            .HasColumnName("jti");

        builder.Property(e => e.OrganisationId).HasColumnName("organisationId");

        builder.Property(e => e.OrganisationName)
            .IsUnicode(false)
            .HasColumnName("organisationName");

        builder.Property(e => e.Scope)
            .IsUnicode(false)
            .HasColumnName("scope");

        builder.Property(e => e.ServiceId).HasColumnName("serviceId");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.UserId)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("userId");

        builder.HasOne(d => d.Service).WithMany(p => p.Grants)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_grants");
    }
}
