using System.Security.Claims;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Extension functions for <see cref="ClaimsPrincipal"/> instances.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Tries to get the <see cref="ClaimTypes.NameIdentifier"/> claim of a <see cref="ClaimsPrincipal"/> instance.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="userId">Output parameter to capture the user ID if present; otherwise, null.</param>
    /// <returns>
    ///   <para>A value of true when the user ID is present; otherwise, false.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="principal"/> is null.</para>
    /// </exception>
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid? userId)
    {
        ExceptionHelpers.ThrowIfArgumentNull(principal, nameof(principal));

        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        userId = claim is not null
            ? new Guid(claim.Value)
            : null;
        return claim is not null;
    }

    /// <summary>
    /// Gets the <see cref="ClaimTypes.NameIdentifier"/> claim of a <see cref="ClaimsPrincipal"/> instance.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>
    ///   <para>The user ID.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="principal"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the <see cref="ClaimTypes.NameIdentifier"/> claim is missing.</para>
    /// </exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        ExceptionHelpers.ThrowIfArgumentNull(principal, nameof(principal));

        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
             ?? throw new InvalidOperationException("Missing user ID.");
        return new Guid(claim.Value);
    }
}
