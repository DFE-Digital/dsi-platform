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
    ILogger<ChangePasswordEndpoint> logger
    ) : IEndpoint
{
    private const int PasswordHistoryLimit = 3;

    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app"></param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangePassword, async (
            [FromBody] ChangePasswordRequest request,
            [FromServices] ChangePasswordEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(request, cancellationToken))
            .WithName("Change Password")
            .WithTags("Users")
            .WithStandardResponses<ChangePasswordRequest>();
    }

    /// <summary>
    /// Handles the change of a user's password.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IResult> HandleAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing password for user {UserId}", request.UserId);

        var user = await this.GetUserAsync(request.UserId, cancellationToken);
        if (user is null) {
            return Results.NotFound();
        }

        if (!await this.ValidateCurrentPasswordAsync(user, request.CurrentPassword, request.UserId)) {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [nameof(request.CurrentPassword)] = ["The password you entered was not recognised"],
            });
        }

        if (await this.IsAttemptingToReusePasswordAsync(user, request.NewPassword, cancellationToken)) {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [nameof(request.NewPassword)] = ["Cannot reuse a recent password"],
            });
        }

        await this.RotatePasswordHistoryAsync(user, cancellationToken);
        this.UpdateUserPassword(user, request.NewPassword);

        await dbDirectoriesContext.SaveChangesAsync(cancellationToken);
        await this.LogSuccessAsync(request.UserId);

        logger.LogInformation("Successfully changed password for user {UserId}", request.UserId);
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
        string currentPolicyCode = passwordHasher.ResolveUserPolicyCode(
            user.UserPasswordPolicies.Select(p => p.PolicyCode));
        string suppliedHash = passwordHasher.Hash(currentPolicyCode, currentPassword, user.Salt);

        if (suppliedHash != user.Password) {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                EventName = AuditChangePasswordEventNames.IncorrectPassword,
                Message = "Failed changed password - Incorrect current password",
                UserId = userId,
                WasFailure = true,
            });
            return false;
        }

        return true;
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

        string currentPolicyCode = passwordHasher.ResolveUserPolicyCode(
            user.UserPasswordPolicies.Select(p => p.PolicyCode));

        var historyTuples = passwordHistories.Select(ph => (currentPolicyCode, ph.Password, ph.Salt));

        return passwordHasher.IsAttemptingToReusePassword(newPassword, historyTuples);
    }

    private void UpdateUserPassword(UserEntity user, string newPassword)
    {
        string newSalt = passwordHasher.GenerateSalt();
        user.Salt = newSalt;
        user.Password = passwordHasher.HashWithLatestPolicy(newPassword, newSalt);
        user.PasswordResetRequired = false;

        bool hasLatestPolicy = user.UserPasswordPolicies
            .Any(p => p.PolicyCode == passwordHasher.LatestPolicyCode);
        if (!hasLatestPolicy) {
            dbDirectoriesContext.UserPasswordPolicies.Add(new UserPasswordPolicyEntity {
                Id = Guid.NewGuid(),
                Uid = user.Sub,
                PolicyCode = passwordHasher.LatestPolicyCode,
                PasswordHistoryLimit = PasswordHistoryLimit
            });
        }
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
            dbDirectoriesContext.UserPasswordHistories.RemoveRange(
                dbDirectoriesContext.UserPasswordHistories.Where(x => x.PasswordHistoryId == oldestId));
            dbDirectoriesContext.PasswordHistories.RemoveRange(
                dbDirectoriesContext.PasswordHistories.Where(x => x.Id == oldestId));
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
