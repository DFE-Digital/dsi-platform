using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// An endpoint to change the password of a user who is not managed via Entra.
/// </summary>
public sealed class ChangePasswordEndpoint(
    DbDirectoriesContext dbDirectoriesContext,
    IAuditWriter auditWriter,
    IPasswordHasher passwordHasher,
    ILogger<ChangePasswordEndpoint> logger) : IEndpoint
{
    private const int PasswordHistoryLimit = 3;

    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app"></param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangePassword, async (
            [FromRoute] Guid userId,
            [FromBody] ChangePasswordRequest request,
            [FromServices] ChangePasswordEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, request, cancellationToken))
            .WithName("Change Password")
            .WithTags("Users")
            .WithStandardResponses()
            .WithValidationFilter<ChangePasswordRequest>();
    }

    /// <summary>
    /// Handles the change of a user's password.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose password is to be changed.</param>
    /// <param name="request">The request containing the current and new passwords.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<IResult> HandleAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing password for user {UserId}", userId);

        var user = await this.GetUserAsync(userId, cancellationToken);
        if (user is null) {
            return Results.NotFound();
        }

        if (!await this.ValidateCurrentPasswordAsync(user, request.CurrentPassword, userId)) {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [nameof(request.CurrentPassword)] = ["The password you entered was not recognised"],
            });
        }

        if (await this.IsAttemptingToReusePasswordAsync(user, request.NewPassword, cancellationToken)) {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [nameof(request.NewPassword)] = ["Your new password cannot be one you have used recently"],
            });
        }

        await this.RotatePasswordHistoryAsync(user, cancellationToken);
        this.UpdateUserPassword(user, request.NewPassword);

        await dbDirectoriesContext.SaveChangesAsync(cancellationToken);
        await this.LogSuccessAsync(userId);

        logger.LogInformation("Successfully changed password for user {UserId}", userId);
        return Results.Ok();
    }

    private async Task<UserEntity?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbDirectoriesContext.Users
            .Include(x => x.UserPasswordPolicies)
            .FirstOrDefaultAsync(x => x.Sub == userId, cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", userId);
        }

        return user;
    }

    private async Task<bool> ValidateCurrentPasswordAsync(UserEntity user, string currentPassword, Guid userId)
    {
        var currentPolicyCode = passwordHasher.ResolveUserPolicyCode(user.UserPasswordPolicies.Select(p => p.PolicyCode));
        var suppliedHash = passwordHasher.Hash(currentPolicyCode, currentPassword, user.Salt);

        if (suppliedHash == user.Password) {
            return true;
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            EventName = AuditChangePasswordEventNames.IncorrectPassword,
            Message = "Failed changed password - Incorrect current password",
            UserId = userId,
            WasFailure = true,
        });

        return false;
    }

    private async Task<bool> IsAttemptingToReusePasswordAsync(UserEntity user, string newPassword, CancellationToken cancellationToken)
    {
        var historyIds = await dbDirectoriesContext.UserPasswordHistories
            .Where(x => x.UserSub == user.Sub)
            .Select(x => x.PasswordHistoryId)
            .ToListAsync(cancellationToken);

        if (historyIds.Count == 0) {
            return false;
        }

        var passwordHistories = await dbDirectoriesContext.PasswordHistories
            .Where(x => historyIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var currentPolicyCode = passwordHasher.ResolveUserPolicyCode(user.UserPasswordPolicies.Select(p => p.PolicyCode));
        var historyTuples = passwordHistories.Select(ph => (currentPolicyCode, ph.Password, ph.Salt));

        return passwordHasher.IsAttemptingToReusePassword(newPassword, historyTuples);
    }

    private void UpdateUserPassword(UserEntity user, string newPassword)
    {
        var newSalt = passwordHasher.GenerateSalt();
        user.Salt = newSalt;
        user.Password = passwordHasher.HashWithLatestPolicy(newPassword, newSalt);
        user.PasswordResetRequired = false;

        var hasLatestPolicy = user.UserPasswordPolicies.Any(p => p.PolicyCode == passwordHasher.LatestPolicyCode);
        if (hasLatestPolicy) {
            return;
        }

        dbDirectoriesContext.UserPasswordPolicies.Add(new UserPasswordPolicyEntity {
            Id = Guid.NewGuid(),
            Uid = user.Sub,
            PolicyCode = passwordHasher.LatestPolicyCode,
            PasswordHistoryLimit = PasswordHistoryLimit
        });
    }

    private async Task LogSuccessAsync(Guid userId)
    {
        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            Message = "Successfully changed password",
            UserId = userId,
        });
    }

    private async Task RotatePasswordHistoryAsync(UserEntity user, CancellationToken cancellationToken)
    {
        var historyIds = await dbDirectoriesContext.UserPasswordHistories
            .Where(x => x.UserSub == user.Sub)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.PasswordHistoryId)
            .Select(x => x.PasswordHistoryId)
            .ToListAsync(cancellationToken);

        if (historyIds.Count >= PasswordHistoryLimit) {
            var oldestId = historyIds[0];

            var userPasswordHistoriesQuery = dbDirectoriesContext.UserPasswordHistories.Where(x => x.PasswordHistoryId == oldestId);
            dbDirectoriesContext.UserPasswordHistories.RemoveRange(userPasswordHistoriesQuery);

            var passwordHistoriesQuery = dbDirectoriesContext.PasswordHistories.Where(x => x.Id == oldestId);
            dbDirectoriesContext.PasswordHistories.RemoveRange(passwordHistoriesQuery);
        }

        var newHistoryId = Guid.NewGuid();
        dbDirectoriesContext.PasswordHistories.Add(new PasswordHistoryEntity {
            Id = newHistoryId,
            Password = user.Password,
            Salt = user.Salt,
        });

        dbDirectoriesContext.UserPasswordHistories.Add(new UserPasswordHistoryEntity {
            PasswordHistoryId = newHistoryId,
            UserSub = user.Sub,
        });
    }
}
