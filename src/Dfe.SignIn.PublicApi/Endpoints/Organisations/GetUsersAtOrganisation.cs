using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

public static partial class OrganisationEndpoints
{
    /// <summary>
    /// Get users and their roles on the service in the bearer token at the organisation.
    /// </summary>
    /// <param name="ukprn"></param>
    /// <param name="clientSession"></param>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<GetUsersAtOrganisationResponse>, NotFound>> GetUsersAtOrganisation(
        int ukprn,
        IClientSession clientSession,
        IInteractionDispatcher interaction)
    {
        try {

            // ^^^^^^^^^^^^^^^^^^^ get the client name, extracted from the bearer token, and lookup client id
            string clientName = clientSession.ClientId;
            Console.WriteLine($"Client name = {clientName}");
            GetApplicationByClientIdResponse applicationResponse;
            applicationResponse = await interaction.DispatchAsync(new GetApplicationByClientIdRequest { ClientId = clientName }).To<GetApplicationByClientIdResponse>();

            var application = applicationResponse.Application;
            if (application == null) {
                return TypedResults.NotFound();
            }

            Console.WriteLine($"Client id = {application.Id}");

            // ^^^^^^^^^^^^^^^^ get matching organisation ids from the UKPRN or UPIN
            GetOrganisationIdsByExternalIdRequest model = new() { LookupKey = "UKPRN-multi", LookupValue = ukprn.ToString() };
            var responseOrgIds = await interaction.DispatchAsync(model).To<GetOrganisationIdsByExternalIdResponse>();
            Console.WriteLine($"First organisation id = {responseOrgIds.OrganisationIds.FirstOrDefault()}");

            // ^^^^^^^^^^^^^^^^^ get user ids of service at organisation
            IOrderedEnumerable<Guid> userIds = Enumerable.Empty<Guid>().OrderBy(x => x);
            foreach (Guid orgId in responseOrgIds.OrganisationIds) {
                Console.WriteLine($"organisation id = {orgId}");

                GetServiceUsersAtOrganisationRequest serviceUsers = new() { OrganisationId = orgId, ApplicationId = application.Id };
                GetServiceUsersAtOrganisationResponse serviceResponse = await interaction.DispatchAsync(serviceUsers).To<GetServiceUsersAtOrganisationResponse>();
                userIds = serviceResponse.UserIds;

                foreach (var userId in userIds) {
                    Console.WriteLine($"userId = {userId}");

                    // ^^^^^^^^^^^^^^^^^^^^^^^^^ get service roles for user at organisation
                    var rolesResponse = await interaction.DispatchAsync(
                        new GetRolesOfUserRequest {
                            ApplicationId = application.Id,
                            OrganisationId = responseOrgIds.OrganisationIds.FirstOrDefault(),
                            UserId = userId
                        }).To<GetRolesOfUserResponse>();
                }
            }

            //return TypedResults.Ok(response);
            return TypedResults.NotFound();
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
