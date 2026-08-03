namespace Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;

/// <summary>
/// Represents the response returned when retrieving a user's profile information.
/// </summary>
public sealed record GetUserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string EmailAddress,
    bool IsEntra,
    bool IsInternalUser,
    string? JobTitle,
    short Status
);
