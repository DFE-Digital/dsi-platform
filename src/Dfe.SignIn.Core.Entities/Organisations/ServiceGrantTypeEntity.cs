using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class ServiceGrantTypeEntity
{
    public Guid ServiceId { get; set; }

    public string GrantType { get; set; } = null!;

    public virtual ServiceEntity Service { get; set; } = null!;
}
#pragma warning restore CS1591

