using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class OrganisationAssociationEntity
{
    public Guid OrganisationId { get; set; }

    public Guid AssociatedOrganisationId { get; set; }

    public string LinkType { get; set; } = null!;

    public virtual OrganisationEntity AssociatedOrganisation { get; set; } = null!;

    public virtual OrganisationEntity Organisation { get; set; } = null!;
}
#pragma warning restore CS1591

