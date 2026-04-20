using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations ids.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationIdsNodeRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient organisationsClient
) : Interactor<GetOrganisationIdsByExternalIdRequest, GetOrganisationIdsByExternalIdResponse>
{
    /// <inheritdoc/>
    public override async Task<GetOrganisationIdsByExternalIdResponse> InvokeAsync(
        InteractionContext<GetOrganisationIdsByExternalIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await organisationsClient.GetFromJsonOrDefaultAsync<Models.OrganisationDto[]>(
             $"/organisations/by-external-id/UKPRN-multi/{context.Request.LookupValue}",
             cancellationToken
        );

        var organisations = response?.Select(org => org.MapToOrganisation(Core.Public.OrganisationStatus.Open, "Establishment", null)) ?? [];

        var orgIds = organisations.Select(x => x.Id);

        return new GetOrganisationIdsByExternalIdResponse {
            OrganisationIds = orgIds
        };
    }
}
