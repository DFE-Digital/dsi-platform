using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.UserCode;

/// <summary>
/// Defines a service for managing user verification codes related to changing email addresses.
/// This service provides methods to delete existing codes and create new verification codes,
/// ensuring that only one valid code exists for a user at any given time.
/// It also logs audit events related to the creation of verification codes.
/// </summary>
public interface IUserCodeService
{
    /// <summary>
    /// Deletes any existing verification codes for changing the email address of a user from the database. This method is typically called before creating a new verification code to ensure that only one valid code exists for the user at any given time.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteExistingCodesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new verification code for changing the email address of a user and persists it to the database. The code is generated using a secure random generator and is associated with the specified user ID, new email address, and client ID. After saving the code, an audit log entry is created to record the event.
    /// </summary>
    /// <param name="userInfo"></param>
    /// <param name="newEmailAddress">The new email address to associate with the verification code.</param>
    /// <param name="clientId">The client ID associated with the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateNewVerificationCodeAsync(UserInfo userInfo, string newEmailAddress, string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the pending change email code for the specified user, if one exists.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, returning the pending code or null.</returns>
    Task<UserCodeEntity?> GetPendingChangeEmailCodeAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// A service that manages user verification codes for changing email addresses.
/// It provides methods to delete existing codes and create new verification codes,
/// ensuring that only one valid code exists for a user at any given time.
/// The service also logs audit events related to the creation of verification codes.
/// </summary>
/// <param name="dbDirectoriesContext">The database context for accessing user codes.</param>
/// <param name="auditWriter">The audit writer for logging audit events.</param>
/// <param name="notificationService">The notification service for sending notifications.</param>
/// <param name="logger">The logger for logging information and errors.</param>
public class UserCodeService(
    DbDirectoriesContext dbDirectoriesContext,
    IAuditWriter auditWriter,
    INotificationService notificationService,
    ILogger<UserCodeService> logger
    ) : IUserCodeService
{
    private const string ChangeEmailCodeType = "changeemail";

    /// <inheritdoc/>
    public async Task<UserCodeEntity?> GetPendingChangeEmailCodeAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbDirectoriesContext.UserCodes
            .Where(x => x.Uid == userId)
            .Where(x => x.CodeType == ChangeEmailCodeType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteExistingCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        try {
            await dbDirectoriesContext.UserCodes
                .Where(x => x.Uid == userId)
                .Where(x => x.CodeType == ChangeEmailCodeType)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (DbUpdateException ex) {
            logger.LogError(ex, "Error deleting existing user codes for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task CreateNewVerificationCodeAsync(
        UserInfo userInfo,
        string newEmailAddress,
        string clientId,
        CancellationToken cancellationToken)
    {
        const string codeType = ChangeEmailCodeType; //todo: consider moving this to a constant in a shared location if it is used in multiple places

        var verificationCode = CodeGenerator.Generate(8, CodeGenerator.FullCharset);
        var now = DateTime.UtcNow;

        var userCode = new UserCodeEntity {
            Uid = userInfo.UserId,
            CodeType = codeType,
            Code = verificationCode,
            ClientId = clientId,
            RedirectUri = "n/a",
            Email = newEmailAddress,
            ContextData = null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbDirectoriesContext.UserCodes.Add(userCode);

        try {
            await dbDirectoriesContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) {
            logger.LogError(ex, "Error saving new verification code for user {UserId}", userInfo.UserId);
            throw;
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.VerificationCode,
            Message = $"Change email verification code {userCode.Code} sent to email {userCode.Email}. code expiry={userCode.CreatedAt:O}, code type={userCode.CodeType.ToLower()}",
            UserId = userInfo.UserId,
        });

        await this.SendVerifyChangeEmailNotification(userCode.Email, userInfo.FirstName, userInfo.LastName, userCode.Code);
        await this.SendNotifyMigratedEmailNotification(userInfo.EmailAddress, userInfo.FirstName, userInfo.LastName, userCode.Email);
    }

    private async Task SendVerifyChangeEmailNotification(string email, string firstName, string lastName, string code)
    {
        await notificationService.SendAsync(
            recipientEmailAddress: email,
            templateId: "8a6b7625-87d5-41bc-bc58-035343571d81",
            personalisation: new Dictionary<string, dynamic> {
                    { "firstName",  firstName},
                    { "lastName",  lastName},
                    { "code",  code},
                    { "email", email},
                    { "helpUrl", ""},
                    { "returnUrl", ""}
                }
        );
    }

    private async Task SendNotifyMigratedEmailNotification(string email, string firstName, string lastName, string newEmail)
    {
        await notificationService.SendAsync(
            recipientEmailAddress: email,
            templateId: "18e0e804-04c6-4f73-9462-ab3cbf8b990f",
            personalisation: new Dictionary<string, dynamic> {
                    {"firstName",  firstName},
                    {"lastName",  lastName},
                    {"newEmail",  newEmail},
                    {"profileUrl", ""},
                    {"helpUrl", ""}
                }
        );
    }
}
