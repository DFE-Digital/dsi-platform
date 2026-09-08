using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Models;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Services;

/// <summary>
/// Synchronises a confirmed DSI email change with Microsoft Entra ID, including rollback on hard failure.
/// </summary>
public interface IChangeEmailEntraSyncService
{
    /// <summary>
    /// Attempts to sync the confirmed email change with Entra after DSI has been updated.
    /// </summary>
    /// <param name="user">The user whose email was updated in DSI.</param>
    /// <param name="originalEmail">The user's email before confirmation.</param>
    /// <param name="newEmail">The confirmed email address now stored in DSI.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The Entra sync outcome.</returns>
    Task<EntraSyncOutcome> SyncConfirmedEmailChangeAsync(
        UserEntity user,
        string originalEmail,
        string newEmail,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default implementation of <see cref="IChangeEmailEntraSyncService"/>.
/// </summary>
public sealed class ChangeEmailEntraSyncService(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    IEntraChangeEmailService entraChangeEmailService,
    ILogger<ChangeEmailEntraSyncService> logger) : IChangeEmailEntraSyncService
{
    /// <inheritdoc/>
    public async Task<EntraSyncOutcome> SyncConfirmedEmailChangeAsync(
        UserEntity user,
        string originalEmail,
        string newEmail,
        CancellationToken cancellationToken)
    {
        if (!user.IsEntraUser()) {
            return new EntraSyncOutcome(EntraSyncStatus.NotApplicable);
        }

        var entraResult = await entraChangeEmailService.ChangeEmailAsync(user.EntraOid!.Value, newEmail, cancellationToken);
        if (entraResult.IsSuccess) {
            return new EntraSyncOutcome(EntraSyncStatus.Succeeded);
        }

        if (entraResult.Error.Code == EntraEmailErrors.Codes.MfaAuthenticationMethodFailed) {
            logger.LogError(
                "Failed to update authentication method in Entra for user {UserId}: {Error}",
                user.Sub,
                entraResult.Error.Description);
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed changed email to {newEmail} - FailedToUpdateAuthenticationMethodException",
                UserId = user.Sub,
                WasFailure = true,
            });
            return new EntraSyncOutcome(EntraSyncStatus.MfaSyncFailed, entraResult.Error);
        }

        logger.LogError(
            "Failed external authentication sync for user {UserId}: {Error}. Rolling back DB change",
            user.Sub,
            entraResult.Error.Description);
        try {
            user.Email = originalEmail;
            await directoriesDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception rollbackEx) {
            logger.LogCritical(
                rollbackEx,
                "Failed to roll back database write for user {UserId} after Entra sync failure",
                user.Sub);
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EmailChangeFailed,
            Message = $"Failed changed email to {newEmail} - {entraResult.Error.Description}",
            UserId = user.Sub,
            WasFailure = true,
        });

        return new EntraSyncOutcome(EntraSyncStatus.HardFailure, entraResult.Error);
    }
}
