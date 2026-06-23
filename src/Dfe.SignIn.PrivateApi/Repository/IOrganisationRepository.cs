using Dfe.SignIn.PrivateApi.DataModels;

namespace Dfe.SignIn.PrivateApi.Repository;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<UserOrganisationServicesQuery>> SelectOrganisationServicesAndRolesByUserId(string clientName, Guid userId, CancellationToken cancellationToken);

}
