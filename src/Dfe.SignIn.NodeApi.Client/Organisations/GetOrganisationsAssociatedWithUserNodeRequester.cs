using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationsAssociatedWithUserNodeRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient
) : Interactor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetOrganisationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.OrganisationsAssociatedWithUserDto[]>(
            $"/organisations/v2/associated-with-user/{context.Request.UserId}",
            cancellationToken
        );

        var organisations = response?.Select(org => org.Organisation.MapToOrganisationModel()) ?? [];

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
