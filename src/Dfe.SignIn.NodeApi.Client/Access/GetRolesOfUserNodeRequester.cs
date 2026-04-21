using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for obtaining roles of a specific user for an application at an organisation.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetRolesOfUserNodeRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient accessClient
) : Interactor<GetRolesOfUserRequest, GetRolesOfUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetRolesOfUserResponse> InvokeAsync(
        InteractionContext<GetRolesOfUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await accessClient.GetFromJsonOrDefaultAsync<Models.ApplicationUserRoleDto>(
            $"users/{context.Request.UserId}/services/{context.Request.ApplicationId}/organisations/{context.Request.OrganisationId}",
            cancellationToken
        );

        IEnumerable<string> roles = response?.Roles?.Select(x => x.Name) ?? [];

        return new GetRolesOfUserResponse { Roles = roles };
    }
}
