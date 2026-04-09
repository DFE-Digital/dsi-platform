using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Services;

public static partial class ServiceEndpoints
{
    /// <summary>
    /// Gets a user's access to a service within an organisation, including their roles,
    /// external identifiers, and legacy IDs used by relying-party systems.
    /// </summary>
    /// <returns>
    ///   <para>200 with the access details when the user has access.</para>
    ///   <para>404 when the user does not have access, or the organisation does not exist.</para>
    /// </returns>
    public static async Task<Results<Ok<GetUserAccessToServiceResponse>, NotFound>> GetUserAccessToService(
        Guid serviceId,
        Guid organisationId,
        Guid userId,
        // ---
        IInteractionDispatcher interaction)
    {
        try {
            var response = await interaction.DispatchAsync(
                new GetUserAccessToServiceRequest {
                    ServiceId = serviceId,
                    OrganisationId = organisationId,
                    UserId = userId,
                }
            ).To<GetUserAccessToServiceResponse>();

            return TypedResults.Ok(response);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
