using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetUsersAssociatedWithOrganisationNodeRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient organisationsClient
) : Interactor<GetUsersAssociatedWithOrganisationRequest, GetUsersAssociatedWithOrganisationResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUsersAssociatedWithOrganisationResponse> InvokeAsync(
        InteractionContext<GetUsersAssociatedWithOrganisationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        // var myRequest = new HttpRequestMessage(HttpMethod.Get, $"organisations/by-external-id/UKPRN-multi/10038591");
        // var myResponse = await organisationsClient.SendAsync(myRequest);
        // var myTextPlease = await myResponse.Content.ReadAsStringAsync();
        // Console.WriteLine(myTextPlease);

        var response = await organisationsClient.GetFromJsonOrDefaultAsync<Models.OrganisationDto[]>(
             $"/organisations/by-external-id/UKPRN-multi/{context.Request.Ukprn}",
             cancellationToken
        );

        var organisations = response?.Select(org => org.MapToOrganisation(Core.Public.OrganisationStatus.Open, "Establishment", null)) ?? [];

        return new GetUsersAssociatedWithOrganisationResponse {
            Organisations = organisations
        };
    }
}
