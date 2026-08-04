using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents a request to change the email address of a user.
/// </summary>
/// <param name="UserExists">A value indicating if the user account actually exists.</param>
/// <param name="UserId">The unique ID of the user.</param>
/// <param name="AccountStatus">A value indicating the status of the user account.</param>
public sealed record UserStatusInfo(
    bool UserExists,
    Guid? UserId,
    AccountStatus? AccountStatus)
{
    /// <summary>
    /// A value indicating if the user account actually exists.
    /// </summary>
    /// <returns>A <see cref="UserStatusInfo"/> representing a user that was not found.</returns>
    public static UserStatusInfo NotFound() => new(false, null, null);

    /// <summary>
    /// A value indicating the status of the user account.
    /// </summary>
    /// <param name="userId">The unique ID of the user.</param>
    /// <param name="accountStatus">A value indicating the status of the user account.</param>
    /// <returns>A <see cref="UserStatusInfo"/> representing a user that was found.</returns>
    public static UserStatusInfo Found(Guid userId, AccountStatus? accountStatus)
        => new(true, userId, accountStatus);
}
