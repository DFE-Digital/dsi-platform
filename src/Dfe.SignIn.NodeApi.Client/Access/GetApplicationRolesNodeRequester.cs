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
) : Interactor<GetApplicationRolesRequest, GetApplicationRolesResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationRolesResponse> InvokeAsync(
        InteractionContext<GetApplicationRolesRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await accessClient.GetFromJsonOrDefaultAsync<Models.RoleDto[]>(
            $"services/{context.Request.ApplicationId}/roles",
            cancellationToken
        );

        return new GetApplicationRolesResponse {
            Roles = response?.Select(r => new ApplicationRole {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                NumericId = r.NumericId,
                Status = r.Status.Id,
                Parent = r.Parent != null ? new ApplicationRole {
                    Id = r.Parent.Id,
                    Name = r.Parent.Name,
                    Code = r.Parent.Code,
                    NumericId = r.Parent.NumericId,
                    Status = r.Parent.Status.Id
                } : null
            }) ?? []
        };
    }
}
