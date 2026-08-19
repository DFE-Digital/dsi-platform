using Dfe.SignIn.Core.Contracts.Graph;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// An interface representing Graph abilities.
/// </summary>
public interface IGraphApiChangeUserPersonalDetails
{
    Task ChangeName(Guid userId, string firstName, string lastName, GraphAccessToken? graphAccessToken, CancellationToken cancellationToken = default);
}
