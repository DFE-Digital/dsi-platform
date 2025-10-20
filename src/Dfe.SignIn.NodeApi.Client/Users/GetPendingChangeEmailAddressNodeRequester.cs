using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to get information about a pending user request to change their
/// email address.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class GetPendingChangeEmailAddressNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient httpClient
) : Interactor<GetPendingChangeEmailAddressRequest, GetPendingChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<GetPendingChangeEmailAddressResponse> InvokeAsync(
        InteractionContext<GetPendingChangeEmailAddressRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpoint = $"/usercodes/{context.Request.UserId}/changeemail";

        var response = await httpClient.GetFromJsonOrDefaultAsync<GetPendingChangeEmailAddressDto>(endpoint, cancellationToken);

        return new GetPendingChangeEmailAddressResponse {
            PendingChangeEmailAddress = response is null ? null : new() {
                UserId = context.Request.UserId,
                NewEmailAddress = response.NewEmailAddress,
                VerificationCode = response.VerificationCode,
                ExpiryTimeUtc = response.CreatedAtUtc.AddHours(1),
            },
        };
    }
}
