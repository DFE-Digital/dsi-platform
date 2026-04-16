using Dfe.SignIn.Base.Framework;
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

            return TypedResults.Ok(response);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
