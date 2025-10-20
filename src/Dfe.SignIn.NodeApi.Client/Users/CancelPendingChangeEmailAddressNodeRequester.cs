using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to get information about a pending user request to change their
/// email address.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class CancelPendingChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    IInteractionDispatcher interaction
) : Interactor<CancelPendingChangeEmailAddressRequest, CancelPendingChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<CancelPendingChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<CancelPendingChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userProfile = await interaction.DispatchAsync(new GetUserProfileRequest {
            UserId = context.Request.UserId,
        }, cancellationToken).To<GetUserProfileResponse>();

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.ChangeEmail,
            EventName = AuditChangeEmailEventNames.CancelChangeEmail,
            Message = $"Cancel change email request from {userProfile.EmailAddress} (id: {context.Request.UserId})",
        }, CancellationToken.None);

        string endpoint = $"usercodes/{context.Request.UserId}/changeemail";
        var response = await directoriesClient.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        return new CancelPendingChangeEmailAddressResponse();
    }
}
