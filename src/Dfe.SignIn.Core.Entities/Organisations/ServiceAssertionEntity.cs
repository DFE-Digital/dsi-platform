using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class ServiceAssertionEntity
{
    public Guid Id { get; set; }

    public Guid ServiceId { get; set; }

    public string TypeUrn { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? FriendlyName { get; set; }

    public virtual ServiceEntity Service { get; set; } = null!;
}
#pragma warning restore CS1591

