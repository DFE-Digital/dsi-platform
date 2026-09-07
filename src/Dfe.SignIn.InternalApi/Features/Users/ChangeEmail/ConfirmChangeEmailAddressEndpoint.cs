using System.Globalization;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to confirm the change of a user's email address.
/// </summary>
public sealed class ConfirmChangeEmailAddressEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    IUserCodeService userCodeService,
    IEntraChangeEmailService entraChangeEmailService,
    IUserUpdatedPublisher userUpdatedPublisher,
    ILogger<ConfirmChangeEmailAddressEndpoint> logger) : IEndpoint
{
    private const int VerificationCodeExpiryHours = 1;

    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ConfirmChangeEmail, async (
            [FromRoute] Guid userId,
            [FromBody] ConfirmChangeEmailAddressRequest request,
            [FromServices] ConfirmChangeEmailAddressEndpoint endpoint,
            CancellationToken cancellationToken) =>
            await endpoint.HandleAsync(userId, request, cancellationToken))
            .WithName("Confirm Change Email Address")
            .WithTags("Users")
            .WithStandardResponses()
            .WithValidationFilter<ConfirmChangeEmailAddressRequest>();
    }

    /// <summary>
    /// Handles the confirmation of a user's email address change request.
    /// </summary>
    public async Task<IResult> HandleAsync(
        Guid userId,
        ConfirmChangeEmailAddressRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirming email change for user {UserId}", userId);

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found when attempting to confirm email change", userId);
            return Results.NotFound();
        }

        var pendingCode = await userCodeService.GetPendingChangeEmailCodeAsync(userId, cancellationToken);
        if (pendingCode is null) {
            logger.LogWarning("No pending email change request found for user {UserId}", userId);
            return Results.ValidationProblem(
                new Dictionary<string, string[]> {
                    [nameof(request.VerificationCode)] = ["No pending change email request found"],
                },
                detail: "No pending change email request found");
        }

        var validationResult = await this.ValidatePendingCodeAsync(request.VerificationCode, pendingCode, userId);
        if (validationResult.IsFailure) {
            return Results.ValidationProblem(
                new Dictionary<string, string[]> {
                    [nameof(request.VerificationCode)] = [validationResult.Error.Description],
                },
                detail: validationResult.Error.Description);
        }

        string originalEmail = user.Email;
        string newEmail = pendingCode.Email!;

        await this.SaveEmailChangeAsync(user, newEmail, cancellationToken);

        var entraSyncResult = await this.SyncEmailWithEntraAsync(user, originalEmail, newEmail, cancellationToken);
        if (entraSyncResult.IsFailure) {
            return MapEntraFailure(entraSyncResult.Error);
        }

        await this.CompleteEmailChangeAsync(user, newEmail, cancellationToken);

        logger.LogInformation("Successfully confirmed email change to {NewEmail} for user {UserId}", newEmail, userId);
        return Results.Ok();
    }

    private async Task<Result> ValidatePendingCodeAsync(
        string verificationCode,
        UserCodeEntity pendingCode,
        Guid userId)
    {
        if (!string.Equals(verificationCode, pendingCode.Code, StringComparison.OrdinalIgnoreCase)) {
            logger.LogWarning("Invalid verification code provided for user {UserId}", userId);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed changed email to {pendingCode.Email} - invalid code",
                UserId = userId,
                WasFailure = true,
            });
            return Result.Failure(new Error("ChangeEmail.InvalidCode", "The verification code you entered is incorrect"));
        }

        var expiryTime = pendingCode.CreatedAt.AddHours(VerificationCodeExpiryHours);
        if (DateTime.UtcNow > expiryTime) {
            logger.LogWarning("Expired verification code provided for user {UserId}", userId);
            string formattedExpiryTime = expiryTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EnteredExpiredCode,
                Message = $"Verification code {pendingCode.Code} expired at {formattedExpiryTime}",
                UserId = userId,
                WasFailure = true,
            });
            return Result.Failure(new Error("ChangeEmail.CodeExpired", "The verification code has expired"));
        }

        if (string.IsNullOrWhiteSpace(pendingCode.Email)) {
            logger.LogWarning("Pending change email request for user {UserId} has no associated email address", userId);
            return Result.Failure(new Error("ChangeEmail.InvalidRequest", "The pending change email request is invalid."));
        }

        return Result.Success();
    }

    private async Task SaveEmailChangeAsync(UserEntity user, string newEmail, CancellationToken cancellationToken)
    {
        try {
            user.Email = newEmail;
            await directoriesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed database write to change email to {NewEmail} for user {UserId}", newEmail, user.Sub);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed changed email to {newEmail} - {ex.Message}",
                UserId = user.Sub,
                WasFailure = true,
            });
            throw;
        }
    }

    private async Task<Result> SyncEmailWithEntraAsync(
        UserEntity user,
        string originalEmail,
        string newEmail,
        CancellationToken cancellationToken)
    {
        if (!user.IsEntraUser()) {
            return Result.Success();
        }

        var entraResult = await entraChangeEmailService.ChangeEmailAsync(user.EntraOid!.Value, newEmail, cancellationToken);
        if (entraResult.IsSuccess) {
            return Result.Success();
        }

        if (entraResult.Error.Code == EntraEmailErrors.Codes.MfaAuthenticationMethodFailed) {
            logger.LogError("Failed to update authentication method in Entra for user {UserId}: {Error}", user.Sub, entraResult.Error.Description);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed changed email to {newEmail} - FailedToUpdateAuthenticationMethodException",
                UserId = user.Sub,
                WasFailure = true,
            });
            return Result.Failure(entraResult.Error);
        }

        logger.LogError("Failed external authentication sync for user {UserId}: {Error}. Rolling back DB change", user.Sub, entraResult.Error.Description);
        try {
            user.Email = originalEmail;
            await directoriesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception rollbackEx) {
            logger.LogCritical(rollbackEx, "Failed to roll back database write for user {UserId} after Entra sync failure", user.Sub);
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EmailChangeFailed,
            Message = $"Failed changed email to {newEmail} - {entraResult.Error.Description}",
            UserId = user.Sub,
            WasFailure = true,
        });

        return Result.Failure(entraResult.Error);
    }

    private static IResult MapEntraFailure(Error error)
    {
        if (error.Code == EntraEmailErrors.Codes.MfaAuthenticationMethodFailed) {
            return Results.Json(
                new { type = "ChangeEmailAddressAuthenticationMethodError", message = error.Description },
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.InternalServerError(new { message = error.Description });
    }

    private async Task CompleteEmailChangeAsync(
        UserEntity user,
        string newEmail,
        CancellationToken cancellationToken)
    {
        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            Message = $"Successfully changed email to {newEmail}",
            UserId = user.Sub,
            CustomProperties = [
                new("editedFields", new object[]
                    {
                        new { name = "new_email", newValue = newEmail }
                    })
            ]
        });

        try {
            // Publish downstream notification event (legacy userupdated_v1)
            await userUpdatedPublisher.PublishUserUpdatedAsync(user.Sub, newEmail, user.FirstName, user.LastName, user.Status, cancellationToken);

            // Clean up verification code
            await userCodeService.DeleteExistingCodesAsync(user.Sub, cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error executing post-update notification tasks for user {UserId}", user.Sub);
            throw;
        }
    }
}
