using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Repository;

/// <summary>
/// Organisations.
/// </summary>
public interface IOrganisationRepository
{
    /// <summary>
    /// Get organisations by Id.
    /// </summary>
    /// <param name="clientName">E.g. gias.</param>
    /// <param name="externalId">UKPRN or UPIN of the organisation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<GetUsersAtOrganisationResponseRaw> SelectByExternalId(string clientName, string externalId, CancellationToken cancellationToken);

    Task<IEnumerable<GetUserOrganisationService>> SelectOrganisationServicesByUserId(Guid userId, CancellationToken cancellationToken);

}
