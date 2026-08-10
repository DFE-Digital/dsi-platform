using Dfe.SignIn.Core.Contracts.Features.Users.Shared;

namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Service for looking up user information from the database.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Gets the user information based on their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user information, or null if not found.</returns>
    Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a user based on their email address.
    /// </summary>
    /// <param name="emailAddress">The email address of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The status information of the user.</returns>
    Task<UserStatusInfo> GetUserStatusByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default);
}
