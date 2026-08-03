namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Service for looking up user information from the database.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Gets the email address of a user based on their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The email address of the user, or null if not found.</returns>
    Task<string?> GetUserEmailAddressAsync(Guid userId, CancellationToken cancellationToken = default);
}
