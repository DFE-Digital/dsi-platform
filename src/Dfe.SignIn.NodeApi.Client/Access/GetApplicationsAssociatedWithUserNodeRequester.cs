using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Users;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for obtaining applications associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetApplicationsAssociatedWithUserNodeRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient httpClient
) : Interactor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetApplicationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.ApplicationDto[]>(
            $"users/{context.Request.UserId}/services",
            cancellationToken
        );

        return new GetApplicationsAssociatedWithUserResponse {
            UserApplicationMappings = response?.Select(s => new UserApplicationMappingModel {
                UserId = s.UserId,
                AccessGranted = s.AccessGrantedOn,
                OrganisationId = s.OrganisationId,
                ApplicationId = s.ServiceId
            }) ?? []
        };
    }
}
