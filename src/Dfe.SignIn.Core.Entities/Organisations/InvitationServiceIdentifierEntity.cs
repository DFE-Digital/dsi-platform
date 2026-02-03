using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class InvitationServiceIdentifierEntity
{
    public Guid InvitationId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid OrganisationId { get; set; }

    public string IdentifierKey { get; set; } = null!;

    public string IdentifierValue { get; set; } = null!;
}
#pragma warning restore CS1591

