using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserServiceIdentifierEntity
{
    public Guid UserId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid OrganisationId { get; set; }

    public string IdentifierKey { get; set; } = null!;

    public string? IdentifierValue { get; set; }
}
#pragma warning restore CS1591

