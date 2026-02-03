using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PolicyConditionEntity
{
    public Guid Id { get; set; }

    public Guid PolicyId { get; set; }

    public string Field { get; set; } = null!;

    public string Operator { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PolicyEntity Policy { get; set; } = null!;
}
#pragma warning restore CS1591

