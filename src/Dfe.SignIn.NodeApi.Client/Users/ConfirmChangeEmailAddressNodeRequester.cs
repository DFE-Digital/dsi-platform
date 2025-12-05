using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Errors.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the name of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class ConfirmChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<ConfirmChangeEmailAddressRequest, ConfirmChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<ConfirmChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<ConfirmChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var pendingChange = await this.GetPendingChangeEmailAddressAsync(context.Request.UserId, cancellationToken);

        await this.ValidatePendingChangeAsync(context, pendingChange);

        try {
            await this.ChangeEmailAsync(pendingChange, cancellationToken);
            await this.WriteAuditSuccessfullyChangedEmailAsync(pendingChange);
            await this.DeleteVerificationCodeAsync(pendingChange);
        }
        catch (Exception ex) {
            await this.WriteAuditFailedToChangeEmailAsync(pendingChange, ex.Message);
            throw;
        }

        return new ConfirmChangeEmailAddressResponse();
    }

    private async Task<PendingChangeEmailAddress> GetPendingChangeEmailAddressAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest { UserId = userId },
            cancellationToken
        ).To<GetPendingChangeEmailAddressResponse>();

        return pendingChangeEmailAddressResponse.PendingChangeEmailAddress
            ?? throw new NoPendingChangeEmailException();
    }

    private async Task ValidatePendingChangeAsync(
        InteractionContext<ConfirmChangeEmailAddressRequest> context,
        PendingChangeEmailAddress pendingChange)
    {
        if (!string.Equals(context.Request.VerificationCode, pendingChange.VerificationCode, StringComparison.InvariantCultureIgnoreCase)) {
            context.AddValidationError(
                "The verification code you entered is incorrect",
                nameof(context.Request.VerificationCode)
            );
            await this.WriteAuditFailedToChangeEmailAsync(pendingChange, "invalid code");
        }

        if (pendingChange.HasExpired) {
            context.AddValidationError(
                "The verification code has expired",
                nameof(context.Request.VerificationCode)
            );
            await this.WriteAuditVerificationCodeHasExpiredAsync(pendingChange);
        }

        context.ThrowIfHasValidationErrors();
    }

    private async Task WriteAuditVerificationCodeHasExpiredAsync(PendingChangeEmailAddress pendingChange)
    {
        string formattedExpiryTime = pendingChange.ExpiryTimeUtc.ToString(
            "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EnteredExpiredCode,
            Message = $"Verification code {pendingChange.VerificationCode} expired at {formattedExpiryTime}",
            UserId = pendingChange.UserId,
            WasFailure = true,
        }, CancellationToken.None);
    }

    private async Task ChangeEmailAsync(PendingChangeEmailAddress pendingChange, CancellationToken cancellationToken)
    {
        var response = await directoriesClient.PatchAsJsonAsync($"users/{pendingChange.UserId}", new {
            email = pendingChange.NewEmailAddress,
        }, CancellationToken.None);

        if (response.StatusCode == HttpStatusCode.InternalServerError) {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessageDto>(cancellationToken);
            if (error?.Type == "ChangeEmailAddressAuthenticationMethodError") {
                throw new FailedToUpdateAuthenticationMethodException(pendingChange.UserId);
            }
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task DeleteVerificationCodeAsync(PendingChangeEmailAddress pendingChange)
    {
        string endpoint = $"usercodes/{pendingChange.UserId}/changeemail";
        var response = await directoriesClient.DeleteAsync(endpoint, CancellationToken.None);
        response.EnsureSuccessStatusCode();
    }

    private async Task WriteAuditSuccessfullyChangedEmailAsync(PendingChangeEmailAddress pendingChange)
    {
        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            Message = $"Successfully changed email to {pendingChange.NewEmailAddress}",
            UserId = pendingChange.UserId,
            CustomProperties = [
                new("editedFields", new object[] {
                    new {
                        name = "new_email",
                        newValue = pendingChange.NewEmailAddress,
                    },
                }),
            ],
        }, CancellationToken.None);
    }

    private async Task WriteAuditFailedToChangeEmailAsync(PendingChangeEmailAddress pendingChange, string errorMessage)
    {
        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.EmailChangeFailed,
            Message = $"Failed changed email to {pendingChange.NewEmailAddress} - {errorMessage}",
            UserId = pendingChange.UserId,
            WasFailure = true,
        }, CancellationToken.None);
    }
}
