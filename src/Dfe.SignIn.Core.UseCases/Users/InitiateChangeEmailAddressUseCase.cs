using System.Security.Cryptography;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Core.UseCases.Users;

public sealed class InitiateChangeEmailAddressUseCase(IUnitOfWorkDirectories unitOfWork,
    IInteractionDispatcher interaction,
    IInteractionLimiter actionLimiter,
    ILogger<InitiateChangeEmailAddressUseCase> logger
) : Interactor<InitiateChangeEmailAddressRequest, InitiateChangeEmailAddressResponse>
{

    private static readonly string CodeType = "changeemail";

    public override async Task<InitiateChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<InitiateChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userProfile = await unitOfWork.Repository<UserEntity>()
                        .Where(x => x.Sub == context.Request.UserId)
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        var existingUserStatus = await interaction.DispatchAsync(
            new GetUserStatusRequest {
                EmailAddress = context.Request.NewEmailAddress,
            }).To<GetUserStatusResponse>();

        // Check if email address is already associated with a user account.
        if (existingUserStatus.UserExists) {
            if (existingUserStatus.UserId == context.Request.UserId) {
                // User is attempting to change their email address to the same address.
                context.AddValidationError(
                    "Input an email address that is different from your current email address",
                    nameof(context.Request.NewEmailAddress)
                );
                context.ThrowIfHasValidationErrors();
            }

            // The user is attempting to use the email address of an existing user account.
            await interaction.DispatchAsync(
                new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.ChangeEmail,
                    EventName = AuditChangeEmailEventNames.RequestedExistingEmail,
                    Message = $"Request to change email from {userProfile.Email} to existing user {context.Request.NewEmailAddress}",
                    UserId = context.Request.UserId,
                }
            );

            context.AddValidationError(
                "Please enter a valid new email address",
                nameof(context.Request.NewEmailAddress)
            );
            context.ThrowIfHasValidationErrors();
        }

        await actionLimiter.LimitAndThrowAsync(context.Request);

        await interaction.DispatchAsync(
        new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.RequestToChangeEmail,
            Message = $"Request to change email from {userProfile.Email} to {context.Request.NewEmailAddress}",
            UserId = context.Request.UserId,
        }
    );

        /*var response = await directoriesClient.PutAsJsonAsync($"usercodes/upsert", new {
            uid = request.UserId.ToString(),
            clientId = request.ClientId,
            redirectUri = "n/a",
            codeType = "changeemail",
            email = request.NewEmailAddress,
            selfInvoked = request.IsSelfInvoked,
        }, CancellationToken.None);
*/

        await this.DeleteAnyExistingVerificationCodeAsync(context.Request, cancellationToken);

        await this.CreateNewVerificationCodeAsync(
            context.Request.UserId,
            context.Request.ClientId,
            "n/a",
            "changeemail",
            context.Request.NewEmailAddress,
            context.Request.IsSelfInvoked);
    }

    private async Task DeleteAnyExistingVerificationCodeAsync(
    InitiateChangeEmailAddressRequest request, CancellationToken cancellationToken)
    {
        try {
            var userCodeToRemove = unitOfWork.Repository<UserCodeEntity>()
                .FirstOrDefault(x => x.Uid == request.UserId && x.CodeType == CodeType);

            if (userCodeToRemove is not null) {
                unitOfWork.Remove(userCodeToRemove);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex) {
            //todo correlation Id??
            logger.LogError("Delete User Code failed for request {correlationId}");
            throw;
        }
    }

    private async Task CreateNewVerificationCodeAsync(Guid uid,
        string clientId, string redirectUri, string codeType, string email, bool isSelfInvoked)
    {
        var userCode = await this.GetUserCode(uid, CodeType);

        if (userCode is null && !string.IsNullOrEmpty(email)) {
            userCode = await this.GetUserCodeByEmail(email, codeType);
        }

        if (userCode is null) {
            CreateUserCode(uid, clientId, redirectUri, email, codeType);
        }

        await interaction.DispatchAsync(
          new WriteToAuditRequest {
              EventCategory = AuditEventCategoryNames.ChangeEmail,
              EventName = AuditChangeEmailEventNames.VerificationCode,
              Message = $"Change email verification code ${code.code} sent to email ${code.email}. code expiry=${code.createdAt}, code type=${code.codeType.toLowerCase()}",
              UserId = request.UserId
          });

        var response = await directoriesClient.PutAsJsonAsync($"usercodes/upsert", new {
            uid = request.UserId.ToString(),
            clientId = request.ClientId,
            redirectUri = "n/a",
            codeType = "changeemail",
            email = request.NewEmailAddress,
            selfInvoked = request.IsSelfInvoked,
        }, CancellationToken.None);

        response.EnsureSuccessStatusCode();
    }

    private async Task<string?> CreateUserCode(Guid userId, string clientId, string
        redirectUri, string email, string codeType)
    {
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri)) {
            return null;
        }

        var code = this.GenerateResetCode();
    }

    private async Task<string?> GetUserCode(Guid userId, string codeType)
    {
        var userCodeToRemove = await unitOfWork.Repository<UserCodeEntity>()
            .Where(x => x.Uid == userId && x.CodeType == codeType)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();

        return userCodeToRemove;
    }

    private async Task<string?> GetUserCodeByEmail(string email, string codeType)
    {
        return (await unitOfWork.Repository<UserCodeEntity>()
            .FirstOrDefaultAsync(x => x.Email == email && x.CodeType == codeType))?.Code ?? null;
    }

    private string GenerateResetCode()
    {
        return CodeGenerator().Ge   
    }
}

public static class CodeGenerator
{
    public const string NumericCharset = "123456789";

    public const string DecCharset = "46789BCDFGHJKLMNPRSTVWXY";

    public const string FullCharset =
        "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.<>!@%&*+_";

    private static double GetRandom()
    {
        // Equivalent to:
        // crypto.randomBytes(4).readUInt32LE() / 0x100000000

        uint value = (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
        return value / 4294967296.0;
    }

    public static string Generate(int length = 8, string? charset = null)
    {
        charset ??= DecCharset;

        var secret = string.Empty;

        for (int i = 0 ; i < length ; i++) {
            secret += charset[(int)Math.Floor(GetRandom() * charset.Length)];
        }

        return secret;
    }
}
