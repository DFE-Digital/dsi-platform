using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// sjw 5 and 6
/// </summary>
/// <param name="Ukprn"></param>
[AssociatedResponse(typeof(GetUsersAtOrganisationResponse))]
public record GetUsersAtOrganisationRequest(
    int Ukprn
);

public record GetUsersAtOrganisationResponse(
    int Ukprn,
    IReadOnlyList<UserAtOrganisation> Users);

public record UserAtOrganisation(
    string Email,
    string FirstName,
    string LastName,
    int UserStatus,
    IReadOnlyList<string> Roles);
