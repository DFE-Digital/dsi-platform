using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request and response models for getting users at an organisation.
/// </summary>
/// <param name="Ukprn"></param>
[AssociatedResponse(typeof(GetUsersAtOrganisationResponse))]
public record GetUsersAtOrganisationRequest(
    int Ukprn
);

/// <summary>
/// Represents the response containing users associated with an organisation.
/// </summary>
/// <param name="Ukprn">
/// The UK Provider Reference Number (UKPRN) identifying the organisation.
/// </param>
/// <param name="Users">
/// A read-only list of users that belong to the organisation.
/// </param>
public record GetUsersAtOrganisationResponse(
    string Ukprn,
    IReadOnlyList<UserAtOrganisation> Users);

/// <summary>
/// Represents a user associated with an organisation.
/// </summary>
/// <param name="Email">
/// The user's email address.
/// </param>
/// <param name="FirstName">
/// The user's first name.
/// </param>
/// <param name="LastName">
/// The user's last name.
/// </param>
/// <param name="UserStatus">
/// The numeric status code representing the user's current state.
/// </param>
/// <param name="Roles">
/// A read-only list of roles assigned to the user within the organisation.
/// </param>
public record UserAtOrganisation(
    string Email,
    string FirstName,
    string LastName,
    int UserStatus,
    IReadOnlyList<string> Roles);
