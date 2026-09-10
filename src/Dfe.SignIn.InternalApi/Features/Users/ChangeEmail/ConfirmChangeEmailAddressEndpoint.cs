using System.Globalization;
using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Models;
using Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Services;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Dfe.SignIn.WebFramework.Validation;
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
    IEntraEmailUpdater entraEmailUpdater,
    IUserUpdatedPublisher userUpdatedPublisher,
    TimeProvider timeProvider,
    ILogger<ConfirmChangeEmailAddressEndpoint> logger) : IEndpoint
{
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
            .WithStandardResponses<ConfirmChangeEmailAddressResponse>()
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

        var validationResult = await this.ValidatePendingEmailChangeAsync(user, request.VerificationCode, cancellationToken);
        if (validationResult.IsFailure) {
            return validationResult.Error.ToValidationProblem(nameof(request.VerificationCode));
        }

        var pendingCode = validationResult.Value;
        string originalEmail = user.Email;
        string newEmail = pendingCode.Email!;

        await this.SaveEmailChangeAsync(user, newEmail, cancellationToken);

        var entraUpdateResult = await entraEmailUpdater.UpdateAsync(
            user,
            originalEmail,
            newEmail,
            cancellationToken);

        if (entraUpdateResult.IsSuccess) {
            return await this.CompleteSuccessfulChangeAsync(user, newEmail, cancellationToken);
        }

        if (entraUpdateResult.Status == EntraEmailUpdateStatus.MfaSyncFailed) {
            return await this.CompleteMfaSyncFailedChangeAsync(user, newEmail, entraUpdateResult.Error!, cancellationToken);
        }

        return Results.Problem(
                detail: entraUpdateResult.Error!.Description,
                statusCode: StatusCodes.Status500InternalServerError);
    }

    private async Task<IResult> CompleteMfaSyncFailedChangeAsync(
        UserEntity user,
        string newEmail,
        Error entraError,
        CancellationToken cancellationToken)
    {
        // Parity with legacy Node: code is cleaned up, but Service Bus notification is skipped
        await userCodeService.DeleteExistingCodesAsync(user.Sub, cancellationToken);

        return Results.Ok(new ConfirmChangeEmailAddressResponse {
            NewEmailAddress = newEmail,
            Warnings = [ChangeEmailWarnings.EntraMfaSyncFailedWarning(entraError.Description)],
        });
    }

    private async Task<IResult> CompleteSuccessfulChangeAsync(
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
            await userUpdatedPublisher.PublishUserUpdatedAsync(
                user.Sub,
                newEmail,
                user.FirstName,
                user.LastName,
                user.Status,
                cancellationToken);

            await userCodeService.DeleteExistingCodesAsync(user.Sub, cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error executing post-update notification tasks for user {UserId}", user.Sub);
            throw;
        }

        logger.LogInformation("Successfully confirmed email change to {NewEmail} for user {UserId}", newEmail, user.Sub);

        return Results.Ok(new ConfirmChangeEmailAddressResponse {
            NewEmailAddress = newEmail,
        });
    }

    private async Task<Result<UserCodeEntity>> ValidatePendingEmailChangeAsync(
        UserEntity user,
        string verificationCode,
        CancellationToken cancellationToken)
    {
        var pendingCode = await userCodeService.GetPendingChangeEmailCodeAsync(user.Sub, cancellationToken);
        if (pendingCode is null) {
            logger.LogWarning("No pending email change request found for user {UserId}", user.Sub);
            return Result.Failure<UserCodeEntity>(ChangeEmailErrors.NoPendingRequest);
        }

        if (!string.Equals(verificationCode, pendingCode.Code, StringComparison.OrdinalIgnoreCase)) {
            logger.LogWarning("Invalid verification code provided for user {UserId}", user.Sub);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed to change email to {pendingCode.Email} - invalid code",
                UserId = user.Sub,
                WasFailure = true,
            });
            return Result.Failure<UserCodeEntity>(ChangeEmailErrors.InvalidCode);
        }

        var expiryTime = pendingCode.CreatedAt.AddHours(ChangeEmailConstants.VerificationCodeExpiryHours);
        if (timeProvider.GetUtcNow().UtcDateTime > expiryTime) {
            logger.LogWarning("Expired verification code provided for user {UserId}", user.Sub);
            string formattedExpiryTime = expiryTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EnteredExpiredCode,
                Message = $"Verification code {pendingCode.Code} expired at {formattedExpiryTime}",
                UserId = user.Sub,
                WasFailure = true,
            });
            return Result.Failure<UserCodeEntity>(ChangeEmailErrors.CodeExpired);
        }

        if (string.IsNullOrWhiteSpace(pendingCode.Email)) {
            logger.LogWarning("Pending change email request for user {UserId} has no associated email address", user.Sub);
            return Result.Failure<UserCodeEntity>(ChangeEmailErrors.InvalidRequest);
        }

        return Result.Success(pendingCode);
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
                Message = $"Failed to change email to {newEmail} - {ex.Message}",
                UserId = user.Sub,
                WasFailure = true,
            });
            throw;
        }
    }
}
