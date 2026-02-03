using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PpOrgAssoCacheEntity
{
    public string MasterProviderCode { get; set; } = null!;

    public string AssociatedMasterProviderCode { get; set; } = null!;

    public string LinkType { get; set; } = null!;
}
#pragma warning restore CS1591

