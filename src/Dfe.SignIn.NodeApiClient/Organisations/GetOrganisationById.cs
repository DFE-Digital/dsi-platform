using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.Organisations.Interactions;
using Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Organisations;

/// <summary>
/// ApiRequester for obtaining an organisation by its unique identifier.
/// </summary>
/// <param name="httpClient"></param>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationById_ApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient)
    : IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{

    /// <inheritdoc/>
    public async Task<GetOrganisationByIdResponse> InvokeAsync(GetOrganisationByIdRequest request)
    {
        var response = await httpClient.GetFromJsonSafeAsync<Models.OrganisationByIdDto>($"/organisations/{request.OrganisationId}");

        return new GetOrganisationByIdResponse {
            Organisation = response is null ? null : new OrganisationModel {
                Id = response.Id,
                LegalName = response.LegalName ?? string.Empty,
                Name = response.Name
            }
        };
    }
}
