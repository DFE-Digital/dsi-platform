using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to determine if a user exists and retrieve their account status.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class GetUserStatusNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient httpClient
) : Interactor<GetUserStatusRequest, GetUserStatusResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserStatusResponse> InvokeAsync(
        InteractionContext<GetUserStatusRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpoint = context.Request.EntraUserId.HasValue
            ? $"users/by-entra-oid/{context.Request.EntraUserId}"
            : $"users/{context.Request.EmailAddress}";

        var response = (await httpClient.GetFromJsonOrDefaultAsync<GetUserStatusResponseDto>(
            endpoint, cancellationToken))!;

        return new GetUserStatusResponse {
            UserExists = response is not null,
            UserId = response?.Id,
            AccountStatus = response?.Status,
        };
    }
}
