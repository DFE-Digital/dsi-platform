using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

public static partial class OrganisationEndpoints
{
    public static async Task<Results<Ok<GetUsersAtOrganisationResponse>, NotFound>> GetUsersAtOrganisation(
        int ukprn,
        IClientSession clientSession,
        IInteractionDispatcher interaction)
    {
        try {

            GetOrganisationIdsRequest model = new() { LookupKey = "UKPRN-multi", LookupValue = ukprn.ToString() };
            var responseOrgIds = await interaction.DispatchAsync(model).To<GetOrganisationIdsResponse>();
            Console.WriteLine($"First organisation id = {responseOrgIds.OrganisationIds.FirstOrDefault()}");

            string clientName = clientSession.ClientId;
            Console.WriteLine($"Client name = {clientName}");
            GetApplicationByClientIdResponse applicationResponse;
            applicationResponse = await interaction.DispatchAsync(new GetApplicationByClientIdRequest { ClientId = clientName }).To<GetApplicationByClientIdResponse>();

            var application = applicationResponse.Application;
            if (application == null) {
                return TypedResults.NotFound();
            }

            Console.WriteLine($"Client id = {application.Id}");

            string serviceId = "77D6B281-9F8D-4649-84B8-87FC42EEE71D";
            string organisationId = "5CCE9B88-D934-4130-89B9-0001B42B84FE";
            string userId = "F448187C-26BB-4578-94FE-906F0D1BF10A";

            var rolesResponse = await interaction.DispatchAsync(
                new GetRolesOfUserRequest {
                    ApplicationId = Guid.Parse(serviceId),
                    OrganisationId = Guid.Parse(organisationId),
                    UserId = Guid.Parse(userId)
                }
            ).To<GetRolesOfUserResponse>();

            var requestModel = new GetUsersAtOrganisationRequest(ukprn);
            var response = await interaction.DispatchAsync(requestModel).To<GetUsersAtOrganisationResponse>();
            return TypedResults.Ok(response);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
