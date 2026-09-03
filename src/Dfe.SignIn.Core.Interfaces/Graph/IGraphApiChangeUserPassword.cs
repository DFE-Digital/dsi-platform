using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Contracts.Graph;

namespace Dfe.SignIn.Core.Interfaces.Graph;

/// <summary>
/// Represents a service for changing a user password with the Graph API.
/// </summary>
public interface IGraphApiChangeUserPassword
{
    /// <summary>
    /// Initiate change of password using the Graph API.
    /// </summary>
    /// <param name="currentPassword">The user's current password.</param>
    /// <param name="newPassword">The user's new password.</param>
    /// <param name="graphAccessToken">The user's Graph API access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ChangePassword(string currentPassword, string newPassword, GraphAccessToken graphAccessToken, CancellationToken cancellationToken = default);
}
