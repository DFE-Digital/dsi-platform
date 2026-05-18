using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Repository;
using Dfe.SignIn.PublicApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    /// <summary>
    /// Gets the list of organisations a user belongs to.
    /// Hidden organisations (status = 0) are excluded.
    /// </summary>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to, including services and roles.</para>
    ///   <para>404 when the user belongs to no organisations.</para>
    /// </returns>
    public static async Task<Results<Ok<GetUserOrganisationServicesResponse>, NotFound>> GetUserOrganisationServices(
        Guid userId,
        IOrganisationRepository organisationRepository,
        CancellationToken cancellationToken)
    {
        IEnumerable<GetUserOrganisationService> models = await organisationRepository.SelectOrganisationServicesByUserId("gias", userId, cancellationToken);

        IEnumerable<GetUserOrganisationServicesResponse> usesrDtos = models.ToUserDtos();

        // userId is primary key
        return TypedResults.Ok(usesrDtos.SingleOrDefault());

    }
}
