using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.NodeApi.Client.Access.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for getting a user's access to a specific service within an organisation.
/// Calls the Access API: GET /users/{userId}/services/{serviceId}/organisations/{organisationId}
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetUserServiceAccessNodeRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient accessClient
) : Interactor<GetUserServiceAccessRequest, GetUserServiceAccessResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserServiceAccessResponse> InvokeAsync(
        InteractionContext<GetUserServiceAccessRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var dto = await accessClient.GetFromJsonOrDefaultAsync<UserServiceAccessDto>(
            $"users/{context.Request.UserId}/services/{context.Request.ServiceId}/organisations/{context.Request.OrganisationId}",
            cancellationToken
        );

        if (dto is null) {
            return new GetUserServiceAccessResponse { Access = null };
        }

        return new GetUserServiceAccessResponse {
            Access = new UserServiceAccess {
                UserId = dto.UserId,
                ServiceId = dto.ServiceId,
                OrganisationId = dto.OrganisationId,
                Roles = dto.Roles.Select(r => new UserServiceRole {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    NumericId = r.NumericId,
                }),
                Identifiers = dto.Identifiers.Select(i => new UserServiceIdentifier {
                    Key = i.Key,
                    Value = i.Value,
                }),
            }
        };
    }
}
