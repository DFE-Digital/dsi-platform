using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetServiceUsersAtOrganisationNodeRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient organisationsClient
) : Interactor<GetServiceUsersAtOrganisationRequest, GetServiceUsersAtOrganisationResponse>
{
    /// <inheritdoc/>
    public override async Task<GetServiceUsersAtOrganisationResponse> InvokeAsync(
        InteractionContext<GetServiceUsersAtOrganisationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await organisationsClient.GetFromJsonOrDefaultAsync<Models.ServiceUserDto[]>(
            $"/organisations/{context.Request.OrganisationId}/services/{context.Request.ApplicationId}/users",
            cancellationToken
        );

        var userIds = response?.Select(x => x.Id) ?? [];

        // users for the organisation and the service
        return new GetServiceUsersAtOrganisationResponse(userIds);
    }
}
