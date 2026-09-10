using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Models;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Services;

/// <summary>
/// Synchronises a confirmed DSI email change with Microsoft Entra ID, including rollback on hard failure.
/// </summary>
public interface IEntraEmailUpdater
{
    /// <summary>
    /// Attempts to sync the confirmed email change with Entra after DSI has been updated.
    /// </summary>
    /// <param name="user">The user whose email was updated in DSI.</param>
    /// <param name="originalEmail">The user's email before confirmation.</param>
    /// <param name="newEmail">The confirmed email address now stored in DSI.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The Entra sync outcome.</returns>
    Task<EntraEmailUpdateResult> UpdateAsync(
        UserEntity user,
        string originalEmail,
        string newEmail,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default implementation of <see cref="IEntraEmailUpdater"/>.
/// </summary>
public sealed class EntraEmailUpdater(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    IEntraChangeEmailService entraChangeEmailService,
    ILogger<EntraEmailUpdater> logger) : IEntraEmailUpdater
{
    /// <inheritdoc/>
    public async Task<EntraEmailUpdateResult> UpdateAsync(
        UserEntity user,
        string originalEmail,
        string newEmail,
        CancellationToken cancellationToken)
    {
        if (!user.IsEntraUser()) {
            return new EntraEmailUpdateResult(EntraEmailUpdateStatus.NotApplicable);
        }

        var entraResult = await entraChangeEmailService.ChangeEmailAsync(user.EntraOid!.Value, newEmail, cancellationToken);
        if (entraResult.IsSuccess) {
            return new EntraEmailUpdateResult(EntraEmailUpdateStatus.Succeeded);
        }

        if (entraResult.Error.Code == EntraEmailErrors.MfaAuthenticationMethodFailedCode) {
            logger.LogError(
                "Failed to update authentication method in Entra for user {UserId}: {Error}",
                user.Sub,
                entraResult.Error.Description);

            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.EmailChangeFailed,
                Message = $"Failed to change email to {newEmail} - FailedToUpdateAuthenticationMethodException",
                UserId = user.Sub,
                WasFailure = true,
            });

            return new EntraEmailUpdateResult(EntraEmailUpdateStatus.MfaSyncFailed, entraResult.Error);
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
            Message = $"Failed to change email to {newEmail} - {entraResult.Error.Description}",
            UserId = user.Sub,
            WasFailure = true,
        });

        return new EntraEmailUpdateResult(EntraEmailUpdateStatus.HardFailure, entraResult.Error);
    }
}
