using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Service for looking up user information from the database.
/// </summary>
/// <param name="dbDirectoriesContext"> The database context for accessing user information. </param>
public class UserLookupService(DbDirectoriesContext dbDirectoriesContext) : IUserLookupService
{
    /// <summary>
    /// Gets the email address of a user based on their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The email address of the user, or null if not found.</returns>
    public async Task<string?> GetUserEmailAddressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userEmail = await dbDirectoriesContext.Users
            .Where(x => x.Sub == userId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return userEmail;
    }
}
