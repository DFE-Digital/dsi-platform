using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class PolicyEntityConfiguration : IEntityTypeConfiguration<PolicyEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PolicyEntity> builder)
    {
        builder.ToTable("Policy");

        builder.HasIndex(e => e.ApplicationId, "IX_Policy_ApplicationId");

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).HasMaxLength(125);

        builder.Property(e => e.Status).HasDefaultValue((short)1);
    }
}
