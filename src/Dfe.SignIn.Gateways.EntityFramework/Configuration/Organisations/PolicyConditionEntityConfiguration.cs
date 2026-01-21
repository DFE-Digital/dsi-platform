using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class PolicyConditionEntityConfiguration : IEntityTypeConfiguration<PolicyConditionEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<PolicyConditionEntity> builder)
    {
        builder.ToTable("PolicyCondition");

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Field)
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(e => e.Operator)
            .HasMaxLength(25)
            .IsUnicode(false);

        builder.HasOne(d => d.Policy).WithMany(p => p.PolicyConditions)
            .HasForeignKey(d => d.PolicyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_PK_PolicyCondition_Policy");
    }
}
