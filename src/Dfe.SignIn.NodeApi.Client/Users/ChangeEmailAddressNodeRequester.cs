using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to change the name of a user.
/// </summary>
[ApiRequester]
[NodeApi(NodeApiName.Directories)]
public sealed class ChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient directoriesClient,
    ILogger<ChangeEmailAddressNodeRequester> logger
) : Interactor<ChangeEmailAddressRequest, ChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<ChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        return new ChangeEmailAddressResponse();
    }
}
