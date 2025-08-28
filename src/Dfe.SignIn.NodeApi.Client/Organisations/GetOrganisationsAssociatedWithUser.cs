using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationsAssociatedWithUser_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient, IMapper mapper
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

        var organisations = response?.Select(org => mapper.Map<OrganisationModel>(org.Organisation)) ?? [];

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}
