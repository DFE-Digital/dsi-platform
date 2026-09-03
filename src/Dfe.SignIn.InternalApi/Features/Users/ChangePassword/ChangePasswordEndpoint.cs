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
DbDirectoriesContext db,
        IAuditWriter auditWriter,
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
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithValidationFilter<ChangePasswordRequest>()
            .WithOpenApi();
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

        var user = await db.Users
            .Include(x => x.UserPasswordPolicies)
            .FirstOrDefaultAsync(x => x.Sub == request.UserId, cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found", request.UserId);
            return Results.NotFound();
        }

        var passwordHasher = new PasswordHasher();

        string currentPolicyCode = passwordHasher.ResolveUserPolicyCode(
            user.UserPasswordPolicies.Select(p => p.PolicyCode));
        string suppliedHash = passwordHasher.Hash(currentPolicyCode, request.CurrentPassword, user.Salt);

        if (suppliedHash != user.Password) {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                EventName = AuditChangePasswordEventNames.IncorrectPassword,
                Message = "Failed changed password - Incorrect current password",
                UserId = request.UserId,
                WasFailure = true,
            });
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [nameof(request.CurrentPassword)] = ["The password you entered was not recognised"],
            });
        }

        await RotatePasswordHistoryAsync(db, user, cancellationToken);

        string newSalt = passwordHasher.GenerateSalt();
        user.Salt = newSalt;
        user.Password = passwordHasher.HashWithLatestPolicy(request.NewPassword, newSalt);
        user.PasswordResetRequired = false;

        bool hasLatestPolicy = user.UserPasswordPolicies
            .Any(p => p.PolicyCode == passwordHasher.LatestPolicyCode);
        if (!hasLatestPolicy) {
            db.UserPasswordPolicies.Add(new UserPasswordPolicyEntity {
                Id = Guid.NewGuid(),
                Uid = user.Sub,
                PolicyCode = passwordHasher.LatestPolicyCode,
                PasswordHistoryLimit = PasswordHistoryLimit
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangePassword,
            Message = "Successfully changed password",
            UserId = request.UserId,
        });

        logger.LogInformation("Successfully changed password for user {UserId}", request.UserId);
        return Results.Ok();
    }

    private static async Task RotatePasswordHistoryAsync(
        DbDirectoriesContext db, UserEntity user, CancellationToken cancellationToken)
    {
        var historyIds = await db.UserPasswordHistories
            .Where(x => x.UserSub == user.Sub)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.PasswordHistoryId)
            .ToListAsync(cancellationToken);

        if (historyIds.Count >= PasswordHistoryLimit) {
            var oldestId = historyIds[0];
            db.UserPasswordHistories.RemoveRange(
                db.UserPasswordHistories.Where(x => x.PasswordHistoryId == oldestId));
            db.PasswordHistories.RemoveRange(
                db.PasswordHistories.Where(x => x.Id == oldestId));
        }

        var newHistoryId = Guid.NewGuid();
        db.PasswordHistories.Add(new PasswordHistoryEntity {
            Id = newHistoryId,
            Password = user.Password,
            Salt = user.Salt,
        });
        db.UserPasswordHistories.Add(new UserPasswordHistoryEntity {
            PasswordHistoryId = newHistoryId,
            UserSub = user.Sub,
        });
    }
}
