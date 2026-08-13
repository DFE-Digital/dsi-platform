using System.Globalization;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.ExternalAuth;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

/// <summary>
/// An endpoint to confirm the change of a user's email address.
/// </summary>
public sealed class ConfirmChangeEmailAddressEndpoint : IEndpoint
{
    private const int VerificationCodeExpiryHours = 1;

    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ConfirmChangeEmail, Handler)
            .WithName("Confirm Change Email Address")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithValidationFilter<ConfirmChangeEmailAddressRequest>()
            .WithOpenApi();
    }

    /// <summary>
    /// Handles the confirmation of a user's email address change request.
    /// </summary>
    public static async Task<IResult> Handler(
        [FromRoute] Guid userId,
        [FromBody] ConfirmChangeEmailAddressRequest request,
        DbDirectoriesContext directoriesDbContext,
        IAuditWriter auditWriter,
        IUserCodeService userCodeService,
        IExternalAuthService externalAuthService,
        IUserUpdatedPublisher userUpdatedPublisher,
        ILogger<ConfirmChangeEmailAddressEndpoint> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirming email change for user {UserId}", userId);

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            logger.LogWarning("User {UserId} not found when attempting to confirm email change", userId);
            return Results.NotFound(new { Message = "User not found" });
        }

        var pendingCode = await userCodeService.GetPendingChangeEmailCodeAsync(userId, cancellationToken);
        if (pendingCode is null) {
            logger.LogWarning("No pending email change request found for user {UserId}", userId);
            return Results.BadRequest(new { Message = "No pending change email request found" });
        }

        if (!await ValidateCode(request.VerificationCode, pendingCode, userId, logger, auditWriter)) {
            return Results.BadRequest(new { Message = "The verification code you entered is incorrect" });
        }

        if (!await ValidateCodeExpiry(pendingCode, userId, logger, auditWriter)) {
            return Results.BadRequest(new { Message = "The verification code has expired" });
        }

        string originalEmail = user.Email;
        string? newEmailNullable = pendingCode.Email;

        if (string.IsNullOrWhiteSpace(newEmailNullable)) {
            logger.LogWarning("Pending change email request for user {UserId} has no associated email address", userId);
            return Results.BadRequest(new { Message = "The pending change email request is invalid." });
        }

        string newEmail = newEmailNullable;

        try {
            user.Email = newEmail;
            await directoriesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed database write to change email to {NewEmail} for user {UserId}", newEmail, userId);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed changed email to {newEmail} - {ex.Message}",
                UserId = userId,
                WasFailure = true,
            });
            throw;
        }

        // Synchronize with External Auth provider (Entra ID)
        if (user.IsEntra && user.EntraOid.HasValue) {
            try {
                await externalAuthService.ChangeEmailAsync(user.EntraOid.Value, newEmail, cancellationToken);
            }
            catch (FailedToUpdateAuthenticationMethodException ex) {
                // Entra MFA failed - DB changes are NOT rolled back, but we return a legacy-compatible JSON response
                logger.LogError(ex, "Failed to update authentication method in Entra for user {UserId}", userId);
                await auditWriter.Log(new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeEmail,
                    EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                    Message = $"Failed changed email to {newEmail} - FailedToUpdateAuthenticationMethodException",
                    UserId = userId,
                    WasFailure = true,
                });
                return Results.Json(
                    new { type = "ChangeEmailAddressAuthenticationMethodError", message = ex.Message },
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex) {
                // Other Entra updates failed - roll back database write
                logger.LogError(ex, "Failed external authentication sync for user {UserId}. Rolling back DB change", userId);
                try {
                    user.Email = originalEmail;
                    await directoriesDbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception rollbackEx) {
                    logger.LogCritical(rollbackEx, "Failed to roll back database write for user {UserId} after Entra sync failure", userId);
                }

                await auditWriter.Log(new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeEmail,
                    EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                    Message = $"Failed changed email to {newEmail} - {ex.Message}",
                    UserId = userId,
                    WasFailure = true,
                });
                throw;
            }
        }

        // Audit, Publish Notification and Cleanup on Success
        await PublishNotification(auditWriter, userUpdatedPublisher, userCodeService, logger, userId, newEmail, user, cancellationToken);

        logger.LogInformation("Successfully confirmed email change to {NewEmail} for user {UserId}", newEmail, userId);
        return Results.Ok();
    }

    private static async Task<bool> ValidateCode(string verificationCode, UserCodeEntity pendingCode, Guid userId, ILogger logger, IAuditWriter auditWriter)
    {
        if (string.Equals(verificationCode, pendingCode.Code, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        logger.LogWarning("Invalid verification code provided for user {UserId}", userId);

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EmailChangeFailed,
            Message = $"Failed changed email to {pendingCode.Email} - invalid code",
            UserId = userId,
            WasFailure = true,
        });

        return false;
    }

    private static async Task<bool> ValidateCodeExpiry(UserCodeEntity pendingCode, Guid userId, ILogger logger, IAuditWriter auditWriter)
    {
        var expiryTime = pendingCode.CreatedAt.AddHours(VerificationCodeExpiryHours);
        if (DateTime.UtcNow <= expiryTime) {
            return true;
        }

        logger.LogWarning("Expired verification code provided for user {UserId}", userId);

        string formattedExpiryTime = expiryTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EnteredExpiredCode,
            Message = $"Verification code {pendingCode.Code} expired at {formattedExpiryTime}",
            UserId = userId,
            WasFailure = true,
        });

        return false;
    }

    private static async Task PublishNotification(
        IAuditWriter auditWriter,
        IUserUpdatedPublisher userUpdatedPublisher,
        IUserCodeService userCodeService,
        ILogger logger,
        Guid userId,
        string newEmail,
        UserEntity user,
        CancellationToken cancellationToken)
    {
        try {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                Message = $"Successfully changed email to {newEmail}",
                UserId = userId,
                CustomProperties = [
                    new("editedFields", new object[]
                    {
                        new { name = "new_email", newValue = newEmail }
                    })
                ]
            });

            // Publish downstream notification event (legacy userupdated_v1)
            await userUpdatedPublisher.PublishUserUpdatedAsync(userId, newEmail, user.FirstName, user.LastName, user.Status, cancellationToken);

            // Clean up verification code
            await userCodeService.DeleteExistingCodesAsync(userId, cancellationToken);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error executing post-update audit/notification tasks for user {UserId}", userId);
            throw;
        }
    }
}
