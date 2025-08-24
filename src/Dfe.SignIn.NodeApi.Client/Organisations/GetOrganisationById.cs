using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations.Interactions;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining an organisation by its unique identifier.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationById_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient, IMapper mapper
) : Interactor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{

    /// <inheritdoc/>
    public override async Task<GetOrganisationByIdResponse> InvokeAsync(
        InteractionContext<GetOrganisationByIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.OrganisationByIdDto>(
            $"/organisations/{context.Request.OrganisationId}",
            cancellationToken
        );

        return new GetOrganisationByIdResponse {
            Organisation = response is null ? null : mapper.Map<OrganisationModel>(response)
        };
    }
}
