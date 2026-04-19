using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

public static partial class OrganisationEndpoints
{
    public static async Task<Results<Ok<GetUsersAtOrganisationResponse>, NotFound>> GetUsersAtOrganisation(
        int ukprn,
        IInteractionDispatcher interaction)
    {
        try {
            var requestModel = new GetUsersAtOrganisationRequest(ukprn);
            var response = await interaction.DispatchAsync(requestModel).To<GetUsersAtOrganisationResponse>();

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

            return TypedResults.Ok(response);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
