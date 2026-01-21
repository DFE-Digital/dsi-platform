using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class VwCognitiveSearchEntity
{
    public string? Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? SearchableName { get; set; }

    public string Email { get; set; } = null!;

    public string? SearchableEmail { get; set; }

    public string? PrimaryOrganisation { get; set; }

    public string Organisations { get; set; } = null!;

    public string SearchableOrganisations { get; set; } = null!;

    public string OrganisationCategories { get; set; } = null!;

    public string OrganisationIdentifiers { get; set; } = null!;

    public string OrganisationsJson { get; set; } = null!;

    public string Services { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public int? NumberOfSuccessfulLoginsInPast12Months { get; set; }

    public int? StatusLastChangedOn { get; set; }

    public int? StatusId { get; set; }

    public string? PendingEmail { get; set; }

    public string? LegacyUsernames { get; set; }

    public DateTime UpdatedAt { get; set; }
}
#pragma warning restore CS1591

