using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to get information about a pending user request to change their
/// email address.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class CancelPendingChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient httpClient
) : Interactor<CancelPendingChangeEmailAddressRequest, CancelPendingChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<CancelPendingChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<CancelPendingChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpoint = $"/usercodes/{context.Request.UserId}/changeemail";

        var response = await httpClient.DeleteAsync(endpoint, cancellationToken);

        return new CancelPendingChangeEmailAddressResponse();
    }
}
