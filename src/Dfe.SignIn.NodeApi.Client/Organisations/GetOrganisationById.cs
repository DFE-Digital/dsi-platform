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
/// <param name="httpClient"></param>
/// <param name="mapper"></param>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationById_NodeApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient, IMapper mapper)
    : IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{

    /// <inheritdoc/>
    public async Task<GetOrganisationByIdResponse> InvokeAsync(
        GetOrganisationByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonOrDefaultAsync<Models.OrganisationByIdDto>(
            $"/organisations/{request.OrganisationId}",
            cancellationToken
        );

        return new GetOrganisationByIdResponse {
            Organisation = response is null ? null : mapper.Map<OrganisationModel>(response)
        };
    }
}
