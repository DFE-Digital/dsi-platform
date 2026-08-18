using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Users;
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
    /// Gets the user information based on their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user information, or null if not found.</returns>
    public async Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbDirectoriesContext.Users
            .AsNoTracking()
            .Where(x => x.Sub == userId)
            .Select(x => new {
                x.Sub,
                x.Email,
                x.FirstName,
                x.LastName,
                x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            return null;
        }

        return new UserInfo(
            user.Sub,
            user.Email,
            user.FirstName,
            user.LastName,
            EnumHelpers.MapEnum<AccountStatus>((int)user.Status)
        );
    }

    /// <summary>
    /// Checks if a user exists in the database based on their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the user exists, otherwise false.</returns>
    public async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbDirectoriesContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Sub == userId, cancellationToken);
    }

    /// <summary>
    /// Gets the status information of a user based on their email address.
    /// </summary>
    /// <param name="emailAddress">The email address of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The status information of the user.</returns>
    public async Task<UserStatusInfo> GetUserStatusByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        var user = await dbDirectoriesContext.Users
            .AsNoTracking()
            .Where(x => x.Email == emailAddress)
            .Select(x => new {
                x.Sub,
                x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            return UserStatusInfo.NotFound();
        }

        return UserStatusInfo.Found(user.Sub, EnumHelpers.MapEnum<AccountStatus>((int)user.Status));
    }
}

//TODO: Consider moving this when no dependencies remain in the Node project. This is a temporary solution to avoid circular dependencies between the Core and Node projects.
