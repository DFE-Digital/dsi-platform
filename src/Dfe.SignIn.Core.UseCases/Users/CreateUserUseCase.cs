using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Search;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor for creating a new user account.
/// </summary>
/// <remarks>
/// Only to be used after an account has been created in Entra.
/// For "classic" account creation a different use-case should be created,
/// as this will require creating a user password policy, determining
/// which policy is currently configured, and implementing that, along
/// with a supporting salt etc. Likely to require configuration access
/// for activePasswordPolicyCode, passwordHistoryLimit.
/// Review login.dfe.directories for implementation details.
/// </remarks>
public sealed class CreateUserUseCase(
    IInteractionDispatcher interaction,
    IUnitOfWorkDirectories unitOfWork,
    TimeProvider timeProvider
) : Interactor<CreateUserRequest, CreateUserResponse>
{
    /// <inheritdoc/>
    public override async Task<CreateUserResponse> InvokeAsync(
        InteractionContext<CreateUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await unitOfWork.Repository<UserEntity>()
                .Where(x =>
                    x.Email == context.Request.EmailAddress ||
                    x.EntraOid == context.Request.EntraUserId)
                .Select(x => new { x.Email })
                .SingleOrDefaultAsync(cancellationToken);

        if (user is not null) {
            throw user.Email.Equals(context.Request.EmailAddress, StringComparison.OrdinalIgnoreCase)
                ? CannotCreateNewUserException.FromEmailAddress(context.Request.EmailAddress)
                : throw CannotCreateNewUserException.FromEntraUserId(context.Request.EntraUserId);
        }

        // Creates a new user only compatible with Entra i.e. no password, salt etc.
        var newUser = new UserEntity {
            Email = context.Request.EmailAddress,
            EntraLinked = timeProvider.GetUtcNow().DateTime,
            EntraOid = context.Request.EntraUserId,
            FirstName = context.Request.FirstName,
            IsEntra = true,
            LastName = context.Request.LastName,
            Password = "none",
            Salt = string.Empty,
            Status = (int)AccountStatus.Active,
            Sub = Guid.NewGuid(),
        };

        await unitOfWork.AddAsync(newUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await interaction.DispatchAsync(
            new UpdateUserInSearchIndexRequest {
                UserId = newUser.Sub
            }
        );

        return new CreateUserResponse {
            UserId = newUser.Sub
        };
    }
}
