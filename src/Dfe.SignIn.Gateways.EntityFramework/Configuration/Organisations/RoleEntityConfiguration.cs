using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Role");

        builder.HasIndex(e => e.ApplicationId, "IX_Role_ApplicationId");

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsUnicode(false);
        builder.Property(e => e.Name).HasMaxLength(125);

        builder.Property(e => e.Status).HasDefaultValue((short)1);
    }
}
