using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.UserCode;

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
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="newEmailAddress">The new email address to associate with the verification code.</param>
    /// <param name="clientId">The client ID associated with the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateNewVerificationCodeAsync(Guid userId, string newEmailAddress, string clientId, CancellationToken cancellationToken);
}

public class UserCodeService(
    DbDirectoriesContext dbDirectoriesContext,
    IAuditWriter auditWriter,
    ILogger<UserCodeService> logger
    ) : IUserCodeService
{
    private const string ChangeEmailCodeType = "changeemail";

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
    public async Task CreateNewVerificationCodeAsync(Guid userId, string newEmailAddress, string clientId, CancellationToken cancellationToken)
    {
        const string codeType = ChangeEmailCodeType; //todo: consider moving this to a constant in a shared location if it is used in multiple places

        var verificationCode = CodeGenerator.Generate(8, CodeGenerator.FullCharset);
        var now = DateTime.UtcNow;

        dbDirectoriesContext.UserCodes.Add(new() {
            Uid = userId,
            CodeType = codeType,
            Code = verificationCode,
            ClientId = clientId,
            RedirectUri = "n/a",
            Email = newEmailAddress,
            ContextData = null,
            CreatedAt = now,
            UpdatedAt = now,
        });

        try {
            await dbDirectoriesContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) {
            logger.LogError(ex, "Error saving new verification code for user {UserId}", userId);
            throw;
        }

        await auditWriter.Log(new Base.Framework.InteractionContext<WriteToAuditRequest>(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.VerificationCodeSent,
                Message = $"Change email verification code {verificationCode} sent to email {newEmailAddress}. code expiry={now:O}, code type={codeType}",
                UserId = userId,
            }));

        // Node.js sends GOV.Notify immediately after the verification code is persisted.
        // Equivalent Node code:
        //
        // const sendNotification = async (user, code, req, uid) => {
        //   const client = new NotificationClient({
        //     connectionString: config.notifications.connectionString,
        //   });
        //
        //   if (!code || !user) {
        //     return Promise.reject("user code or user object is null");
        //   }
        //
        //   if (code.codeType.toLowerCase() === "changeemail") {
        //     const emailUid = req.body.selfInvoked ? undefined : uid;
        //     await client.sendVerifyChangeEmail(
        //       code.email,
        //       user.given_name,
        //       user.family_name,
        //       code.code,
        //       emailUid,
        //     );
        //     return client.sendNotifyMigratedEmail(
        //       user.email,
        //       user.given_name,
        //       user.family_name,
        //       code.email,
        //     );
        //   }
        // };
        //
        // TODO: wire this notification send back in if GOV.Notify is reintroduced.
    }
}
