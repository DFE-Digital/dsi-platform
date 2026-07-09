using System.Security.Cryptography;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.Users;

internal record UserCodeDto(Guid Uid, string EmailAddress, string Code);
/// <summary>
/// 
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="interaction"></param>
/// <param name="logger"></param>
public sealed class InitiateChangeEmailAddressUseCase(IUnitOfWorkDirectories unitOfWork,
    IInteractionDispatcher interaction,
    IOptions<PlatformOptions> platformOptions,
    ILogger<InitiateChangeEmailAddressUseCase> logger
) : Interactor<InitiateChangeEmailAddressRequest, InitiateChangeEmailAddressResponse>
{

    private static readonly string CodeType = "changeemail";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

        await interaction.DispatchAsync(
        new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.RequestToChangeEmail,
            Message = $"Request to change email from {userProfile.Email} to {context.Request.NewEmailAddress}",
            UserId = context.Request.UserId,
        }
    );

        await this.DeleteAnyExistingVerificationCodeAsync(context.Request, cancellationToken);

        await this.CreateNewVerificationCodeAsync(
            context.Request.UserId,
            context.Request.ClientId,
            "n/a",
            "changeemail",
            context.Request.NewEmailAddress);

        return new InitiateChangeEmailAddressResponse();
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
            logger.LogError("Delete User Code failed");
            throw;
        }
    }

    private async Task CreateNewVerificationCodeAsync(Guid uid,
        string clientId, string redirectUri, string codeType, string email)
    {
        var userCode = await this.GetUserCode(uid, CodeType);

        if (userCode is null && !string.IsNullOrEmpty(email)) {
            userCode = await this.GetUserCodeByEmail(email, codeType);
        }

        if (userCode is null) {
            userCode = await this.CreateUserCode(uid, clientId, redirectUri, email);
        }
        else {
            userCode = await this.UpdateUserCode(uid, email, redirectUri, clientId, codeType);
        }

        var user = await unitOfWork.Repository<UserEntity>().FirstOrDefaultAsync(u => u.Sub == uid)
            ?? throw new Exception("User not found!");

        await interaction.DispatchAsync(new SendEmailNotificationRequest {
            RecipientEmailAddress = email,
            TemplateId = "8a6b7625-87d5-41bc-bc58-035343571d81",
            Personalisation = new Dictionary<string, dynamic> {
                { "firstName",  user.FirstName},
                { "lastName",  user.LastName},
                { "code",  userCode.Code},
                { "email", email},
                { "helpUrl", platformOptions.Value.HelpUrl},
                { "returnUrl", ""}
            }
        });

        await interaction.DispatchAsync(new SendEmailNotificationRequest {
            RecipientEmailAddress = user.Email,
            TemplateId = "18e0e804-04c6-4f73-9462-ab3cbf8b990f",
            Personalisation = new Dictionary<string, dynamic> {
                {"firstName",  user.FirstName},
                {"lastName",  user.LastName},
                {"newEmail",  email},
                {"profileUrl", platformOptions.Value.ProfileUrl },
                {"helpUrl", platformOptions.Value.HelpUrl }
            }
        });
    }

    private async Task<UserCodeDto?> CreateUserCode(Guid userId, string clientId, string
        redirectUri, string email)
    {
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri)) {
            return null;
        }

        var code = this.GenerateResetCode();

        var userCodeEntity = new UserCodeEntity {
            ClientId = clientId,
            Code = code,
            CodeType = "changeemail",
            CreatedAt = DateTime.UtcNow,
            Email = email,
            RedirectUri = redirectUri,
            Uid = userId,
            UpdatedAt = DateTime.UtcNow

        };

        await unitOfWork.AddAsync(userCodeEntity);

        _ = await unitOfWork.SaveChangesAsync();

        return new UserCodeDto(userId, email, code);
    }

    private async Task<UserCodeDto?> GetUserCode(Guid userId, string codeType)
    {
        var userCodeToRemove = await unitOfWork.Repository<UserCodeEntity>()
            .Where(x => x.Uid == userId && x.CodeType == codeType)
            .Select(x => new UserCodeDto(userId, x.Email, x.Code))
            .FirstOrDefaultAsync();

        return userCodeToRemove;
    }

    private async Task<UserCodeDto?> GetUserCodeByEmail(string email, string codeType)
    {
        var usercode = await unitOfWork.Repository<UserCodeEntity>()
             .FirstOrDefaultAsync(x => x.Email == email && x.CodeType == codeType);

        if (usercode is null) {
            return null;
        }

        return new UserCodeDto(usercode.Uid, usercode.Email, usercode.Code);
    }

    private string GenerateResetCode()
    {
        return CodeGenerator.Generate(8, CodeGenerator.FullCharset);
    }

    private async Task<UserCodeDto?> UpdateUserCode(Guid userId, string email, string
        redirectUri, string clientId, string codeType)
    {
        logger.LogInformation("Update User Code");

        var codeFromFind = await this.GetUserCode(userId, codeType);

        if (codeFromFind is null) {
            return null;
        }

        if (string.IsNullOrEmpty(codeFromFind.EmailAddress) || !string.Equals(codeFromFind.EmailAddress, email, StringComparison.OrdinalIgnoreCase)) {
            var code = CodeGenerator.Generate(8, CodeGenerator.FullCharset);

            var userCode = await unitOfWork.Repository<UserCodeEntity>()
                .Where(x => x.Uid == userId && x.CodeType == codeType)
                .FirstOrDefaultAsync();

            if (userCode is null) {
                return null;
            }

            userCode.Email = email;
            userCode.RedirectUri = redirectUri;
            userCode.ClientId = clientId;
            userCode.Code = code;

            _ = await unitOfWork.SaveChangesAsync();

            return new UserCodeDto(userId, email, code);
        }

        return new UserCodeDto(userId, email, codeFromFind.Code);
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
