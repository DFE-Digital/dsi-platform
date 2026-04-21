using Azure;
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

            // ^^^^^^^^^^^^^^^^^^^ use the client name, extracted from the bearer token, to lookup the client id
            string clientName = clientSession.ClientId;
            GetApplicationByClientIdRequest applicationRequest = new() { ClientId = clientName };
            var applicationResponse = await interaction.DispatchAsync(applicationRequest).To<GetApplicationByClientIdResponse>();

            var application = applicationResponse.Application;
            if (application == null) {
                return TypedResults.NotFound();
            }

            Console.WriteLine($"Client id = {application.Id}");

            // ^^^^^^^^^^^^^^^^ get matching organisation ids from the UKPRN or UPIN
            GetOrganisationIdsByExternalIdRequest orgIdsRequest = new() { LookupKey = "UKPRN-multi", LookupValue = ukprn.ToString() };
            var orgIdsResponse = await interaction.DispatchAsync(orgIdsRequest).To<GetOrganisationIdsByExternalIdResponse>();
            Console.WriteLine($"First organisation id = {orgIdsResponse.OrganisationIds.FirstOrDefault()}");

            IOrderedEnumerable<Guid> userIds = Enumerable.Empty<Guid>().OrderBy(x => x);
            foreach (Guid orgId in orgIdsResponse.OrganisationIds) {
                Console.WriteLine($"organisation id = {orgId}");

                // ^^^^^^^^^^^^^^^^^ get user ids of service at organisation
                GetServiceUsersAtOrganisationRequest serviceUsersRequest = new() { OrganisationId = orgId, ApplicationId = application.Id };
                var serviceUsersResponse = await interaction.DispatchAsync(serviceUsersRequest).To<GetServiceUsersAtOrganisationResponse>();
                userIds = serviceUsersResponse.UserIds;

                foreach (var userId in userIds) {
                    Console.WriteLine($"userId = {userId}");

                    // ^^^^^^^^^^^^^^^^^^^^^^^^^ get service roles for user at organisation
                    GetRolesOfUserRequest rolesRequest = new() {
                        ApplicationId = application.Id,
                        OrganisationId = orgId,
                        UserId = userId
                    };

                    var rolesResponse = await interaction.DispatchAsync(rolesRequest).To<GetRolesOfUserResponse>();

                    if (rolesResponse?.Roles != null) {
                        Console.WriteLine($"First role = {rolesResponse.Roles.FirstOrDefault()}");
                    }
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
