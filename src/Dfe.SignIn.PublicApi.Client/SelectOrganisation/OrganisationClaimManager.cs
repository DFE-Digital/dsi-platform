using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service that manages the organisation claim of an authenticated user.
/// </summary>
public interface IOrganisationClaimManager
{
    /// <summary>
    /// Update the organisation claim of the authenticated user.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    /// <param name="organisationJson">The JSON encoded organisation claim value.</param>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the user is not currently authenticated.</para>
    /// </exception>
    Task UpdateOrganisationClaimAsync(HttpContext context, string organisationJson);
}

/// <summary>
/// Concrete implementation of <see cref="IOrganisationClaimManager"/>.
/// </summary>
internal sealed class OrganisationClaimManager : IOrganisationClaimManager
{
    private static void RemoveOrganisationClaim(ClaimsIdentity identity)
    {
        var organisationClaims = identity.Claims
            .Where(claim => claim.Type == DsiClaimTypes.Organisation)
            .ToArray();
        foreach (var oldClaim in organisationClaims) {
            identity.RemoveClaim(oldClaim);
        }
    }

    // Proxy for 'SignInAsync' function since `HttpContext.SignInAsync` is
    // difficult to mock.
    internal Func<HttpContext, ClaimsPrincipal, Task> SignInProxyAsync { get; set; }
        = (context, principal) => context.SignInAsync(principal);

    /// <inheritdoc/>
    public Task UpdateOrganisationClaimAsync(HttpContext context, string organisationJson)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(organisationJson, nameof(organisationJson));

        var identity = (context.User.Identity as ClaimsIdentity)!.Clone();

        RemoveOrganisationClaim(identity);

        var organisationClaim = new Claim("organisation", organisationJson, JsonClaimValueTypes.Json);
        identity.AddClaim(organisationClaim);

        var newPrincipal = new ClaimsPrincipal(identity);
        return this.SignInProxyAsync(context, newPrincipal);
    }
}
