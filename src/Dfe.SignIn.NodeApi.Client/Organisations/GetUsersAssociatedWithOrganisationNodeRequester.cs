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

        // service is GIAS. Need to obtain value form the bearer token
        string serviceId = "77D6B281-9F8D-4649-84B8-87FC42EEE71D";
        string userId = "F448187C-26BB-4578-94FE-906F0D1BF10A";

        var orgIds = organisations.Select(x => x.Id);
        Guid anOrgId;
        foreach (Guid orgId in orgIds) {

            anOrgId = orgId;
            // users for the organisation and the service
            // var myRequest = new HttpRequestMessage(HttpMethod.Get, $"/organisations/{orgId}/services/{serviceId}/users");

            // var myRequest = new HttpRequestMessage(HttpMethod.Get, "services/77D6B281-9F8D-4649-84B8-87FC42EEE71D/organisations/5CCE9B88-D934-4130-89B9-0001B42B84FE/users/F448187C-26BB-4578-94FE-906F0D1BF10A");

            // var myResponse = await organisationsClient.SendAsync(myRequest);
            // var myTextPlease = await myResponse.Content.ReadAsStringAsync();
            // Console.WriteLine(myTextPlease);
        }

        return new GetUsersAssociatedWithOrganisationResponse {
            Organisations = organisations
        };
    }
}
