using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the name of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class InitiateChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction,
    IInteractionLimiter actionLimiter
) : Interactor<InitiateChangeEmailAddressRequest, InitiateChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<InitiateChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<InitiateChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userProfile = await interaction.DispatchAsync(new GetUserProfileRequest {
            UserId = context.Request.UserId,
        }, cancellationToken).To<GetUserProfileResponse>();

        var existingUserStatus = await interaction.DispatchAsync(new GetUserStatusRequest {
            EmailAddress = context.Request.NewEmailAddress,
        }, cancellationToken).To<GetUserStatusResponse>();

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
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.RequestedExistingEmail,
                Message = $"Request to change email from {userProfile.EmailAddress} to existing user {context.Request.NewEmailAddress}",
                UserId = context.Request.UserId,
            }, CancellationToken.None);

            context.AddValidationError(
                "Please enter a valid new email address",
                nameof(context.Request.NewEmailAddress)
            );
            context.ThrowIfHasValidationErrors();
        }

        await actionLimiter.LimitAndThrowAsync(context.Request);

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.RequestToChangeEmail,
            Message = $"Request to change email from {userProfile.EmailAddress} to {context.Request.NewEmailAddress}",
            UserId = context.Request.UserId,
        }, CancellationToken.None);

        await this.DeleteAnyExistingVerificationCodeAsync(context.Request, cancellationToken);
        await this.CreateNewVerificationCodeAsync(context.Request);

        return new InitiateChangeEmailAddressResponse();
    }

    private async Task DeleteAnyExistingVerificationCodeAsync(
        InitiateChangeEmailAddressRequest request, CancellationToken cancellationToken)
    {
        string endpoint = $"usercodes/{request.UserId}/changeemail";
        var response = await directoriesClient.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task CreateNewVerificationCodeAsync(InitiateChangeEmailAddressRequest request)
    {
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
}
