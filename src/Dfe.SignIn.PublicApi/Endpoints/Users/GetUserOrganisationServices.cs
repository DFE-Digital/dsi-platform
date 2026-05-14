using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Repository;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    /// <summary>
    /// Gets the list of organisations a user belongs to.
    /// Hidden organisations (status = 0) are excluded.
    /// </summary>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to at least one
    ///   visible organisation.</para>
    ///   <para>404 when the user belongs to no organisations, or all are hidden.</para>
    /// </returns>
    public static async Task<Results<Ok<IEnumerable<GetUserOrganisationService>>, NotFound>> GetUserOrganisationServices(
        Guid userId,
        IOrganisationRepository organisationRepository,
        CancellationToken cancellationToken)
    {
        IEnumerable<GetUserOrganisationService> models = await organisationRepository.SelectOrganisationServicesByUserId(userId, cancellationToken);

        return TypedResults.Ok(models);

    }
}
