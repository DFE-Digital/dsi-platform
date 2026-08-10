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
///   <para>Only to be used after an account has been created in Entra.
///   For "classic" account creation a different use-case should be created,
///   as this will require creating a user password policy, determining
///   which policy is currently configured, and implementing that, along
///   with a supporting salt etc. Likely to require configuration access
///   for activePasswordPolicyCode, passwordHistoryLimit.
///   Review login.dfe.directories for implementation details.</para>
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

        try {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex)) {
            // A concurrent request for the same Entra identity may have won the race to
            // create this user (e.g. two simultaneous sign-ins for the same account).
            // Email and EntraOid are each independently enforced as unique, so a row
            // matching both values is the unambiguous race winner. A row matching only
            // one of the two (e.g. the email is already used by a different Entra
            // identity) is a genuine conflict, not a self-race - that must surface as
            // the original exception rather than silently resolving to someone else's
            // account.
            var raceWinner = await unitOfWork.Repository<UserEntity>()
                    .Where(x =>
                        x.Email == context.Request.EmailAddress &&
                        x.EntraOid == context.Request.EntraUserId)
                    .Select(x => new { x.Sub })
                    .SingleOrDefaultAsync(cancellationToken);

            if (raceWinner is null) {
                throw;
            }

            return new CreateUserResponse {
                UserId = raceWinner.Sub
            };
        }

        await interaction.DispatchAsync(
            new UpdateUserInSearchIndexRequest {
                UserId = newUser.Sub
            }
        );

        return new CreateUserResponse {
            UserId = newUser.Sub
        };
    }

    // SqlException's constructors are internal, so it can't be inspected by type/Number
    // here without a hard dependency on Microsoft.Data.SqlClient in this layer. Matching
    // on the well-known SQL Server unique-violation message text (2601: unique index,
    // 2627: named unique constraint - this table has both, see UserEntityConfiguration)
    // keeps this scoped to actual unique-constraint races, not any other DbUpdateException
    // cause (e.g. FK violations, timeouts) that recovery-by-lookup would misinterpret.
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key row", StringComparison.OrdinalIgnoreCase);
    }
}
