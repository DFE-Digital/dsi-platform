using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class ToggleFlagEntity
{
    public string Type { get; set; } = null!;

    public string ServiceName { get; set; } = null!;

    public bool Flag { get; set; }
}
#pragma warning restore CS1591

