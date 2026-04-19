using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for obtaining roles of a specific application.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetServiceRolesNodeRequester(
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
            $"services/{context.Request.ApplicationId}/organisations/{context.Request.OrganisationId}/users/{context.Request.UserId}",
            cancellationToken
        );

        /*
        return new GetRolesOfUserResponse {
            Roles = response?.Select(r => new UserRole {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                NumericId = r.NumericId,
                Status = r.Status.Id,
                Parent = r.Parent != null ? new UserRole {
                    Id = r.Parent.Id,
                    Name = r.Parent.Name,
                    Code = r.Parent.Code,
                    NumericId = r.Parent.NumericId,
                    Status = r.Parent.Status.Id
                } : null
            }) ?? []
        };
        */

        List<string> roles = [];
        return new GetRolesOfUserResponse { Roles = roles };
    }
}
