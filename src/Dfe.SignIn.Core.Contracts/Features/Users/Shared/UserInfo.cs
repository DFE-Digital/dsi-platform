using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents information about a user.
/// </summary>
/// <param name="UserId">The unique ID of the user.</param>
/// <param name="EmailAddress">The email address of the user.</param>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Status">The status of the user account.</param>
public sealed record UserInfo(
    Guid UserId,
    string EmailAddress,
    string FirstName,
    string LastName,
    AccountStatus Status);
