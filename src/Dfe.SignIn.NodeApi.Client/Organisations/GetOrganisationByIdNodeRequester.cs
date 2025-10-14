using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining an organisation by its unique identifier.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationByIdNodeRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient organisationsClient
) : Interactor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{

    /// <inheritdoc/>
    public override async Task<GetOrganisationByIdResponse> InvokeAsync(
        InteractionContext<GetOrganisationByIdRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await organisationsClient.GetFromJsonOrDefaultAsync<OrganisationByIdDto>(
            $"/organisations/{context.Request.OrganisationId}",
            cancellationToken
        );

        return new GetOrganisationByIdResponse {
            Organisation = response?.MapToOrganisation()
        };
    }
}
