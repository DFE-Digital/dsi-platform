using System.Security.Claims;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Extension functionality functionality to assist with DfE Sign-in claims.
/// </summary>
/// <seealso cref="DsiClaimTypes"/>
public static class DsiClaimExtensions
{
    /// <summary>
    /// Gets the primary identity from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The claims identity representing the primary user identity when found;
    ///   otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static ClaimsIdentity? GetPrimaryIdentity(this ClaimsPrincipal user)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        return user.Identities.FirstOrDefault(identity =>
            identity.HasClaim(claim => claim.Type == DsiClaimTypes.UserId)
        );
    }

    /// <summary>
    /// Gets the DfE Sign-in user identifier from from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <returns>
    ///   <para>The unique identifier of the user in DfE Sign-in.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    /// <exception cref="MissingClaimException">
    ///   <para>If <paramref name="user"/> does not include the "dsi_user_id" claim.</para>
    /// </exception>
    public static Guid GetDsiUserId(this ClaimsPrincipal user)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        TryGetDsiUserId(user, out var userId);
        return userId ?? throw new MissingClaimException("Missing identity with claim.", DsiClaimTypes.UserId);
    }

    /// <summary>
    /// Tries to get the DfE Sign-in user identifier from a claims principal.
    /// </summary>
    /// <param name="user">Claims principal representing the user.</param>
    /// <param name="userId">Outputs the user ID when present; otherwise, null.</param>
    /// <returns>
    ///   <para>A value of <c>true</c> if the "dsi_user_id" claim exists; otherwise, a
    ///   value of <c>false</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="user"/> is null.</para>
    /// </exception>
    public static bool TryGetDsiUserId(this ClaimsPrincipal user, out Guid? userId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(user, nameof(user));

        var claim = user?.FindFirst(DsiClaimTypes.UserId);
        userId = claim is not null
            ? new Guid(claim.Value)
            : null;
        return userId is not null;
    }
}
