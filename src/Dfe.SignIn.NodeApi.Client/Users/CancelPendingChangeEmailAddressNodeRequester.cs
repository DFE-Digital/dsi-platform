using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
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
    IInteractionDispatcher interaction,
    IUserLookupService userLookupService
) : Interactor<CancelPendingChangeEmailAddressRequest, CancelPendingChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<CancelPendingChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<CancelPendingChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userEmail = await userLookupService.GetUserEmailAddressAsync(context.Request.UserId, cancellationToken);

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.CancelChangeEmail,
                Message = $"Cancel change email request from {userEmail} (id: {context.Request.UserId})",
            }
        );

        string endpoint = $"usercodes/{context.Request.UserId}/changeemail";
        var response = await directoriesClient.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        return new CancelPendingChangeEmailAddressResponse();
    }
}
