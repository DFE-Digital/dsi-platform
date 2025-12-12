using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Users;

/// <summary>
/// An interactor to determine if a user exists and retrieve their account status.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Directories)]
public sealed class GetUserProfileNodeRequester(
    [FromKeyedServices(NodeApiName.Directories)] HttpClient httpClient
) : Interactor<GetUserProfileRequest, GetUserProfileResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserProfileResponse> InvokeAsync(
        InteractionContext<GetUserProfileRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string endpoint = $"users/{context.Request.UserId}";

        var response = (await httpClient.GetFromJsonOrDefaultAsync<GetUserProfileDto>(endpoint, cancellationToken))
            ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        return new GetUserProfileResponse {
            IsEntra = response.IsEntra,
            IsInternalUser = response.IsInternalUser,
            FirstName = response.FirstName,
            LastName = response.LastName,
            JobTitle = !string.IsNullOrWhiteSpace(response.JobTitle) ? response.JobTitle : null,
            EmailAddress = response.EmailAddress,
        };
    }
}
