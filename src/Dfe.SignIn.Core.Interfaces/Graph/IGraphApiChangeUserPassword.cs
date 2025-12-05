using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Interfaces.Graph;

/// <summary>
/// Represents a service for changing a user password with the Graph API.
/// </summary>
public interface IGraphApiChangeUserPassword
{
    /// <summary>
    /// Initiate change of password using the Graph API.
    /// </summary>
    /// <param name="context">A user request to self-change their password.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the request is missing the user access token.</para>
    ///   <para>- or -</para>
    ///   <para>If the confirmed password does not match new password.</para>
    /// </exception>
    Task ChangePassword(InteractionContext<SelfChangePasswordRequest> context);
}
