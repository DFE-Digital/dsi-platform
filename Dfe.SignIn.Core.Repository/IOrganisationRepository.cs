using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Repository;

/// <summary>
/// Organisations.
/// </summary>
public interface IOrganisationRepository
{
    /// <summary>
    /// Given a user identifier, return organisations the user is assigned and their services and roles.
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="cancellationToken">The cancellation token<./param>
    /// <returns></returns>
    Task<IEnumerable<GetUserOrganisationService>> SelectOrganisationServicesAndRolesByUserId(string clientName, Guid userId, CancellationToken cancellationToken);

}
