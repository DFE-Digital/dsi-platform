using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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
internal sealed class OrganisationClaimManager(
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor
) : IOrganisationClaimManager
{
    // Proxy for 'SignInAsync' function since `HttpContext.SignInAsync` is
    // difficult to mock.
    internal Func<HttpContext, ClaimsPrincipal, Task> SignInProxyAsync { get; set; }
        = (context, principal) => context.SignInAsync(principal);

    /// <inheritdoc/>
    public async Task UpdateOrganisationClaimAsync(HttpContext context, string organisationJson)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(organisationJson, nameof(organisationJson));

        var options = optionsAccessor.Value;

        var otherIdentities = context.User.Identities
            .Where(identity => identity.AuthenticationType != PublicApiConstants.AuthenticationType);

        var dsiIdentity = new ClaimsIdentity(PublicApiConstants.AuthenticationType);

        if (options.UpdateClaimsIdentity is not null) {
            await options.UpdateClaimsIdentity.Invoke(dsiIdentity);
        }

        dsiIdentity.AddClaim(
            new Claim("organisation", organisationJson, JsonClaimValueTypes.Json)
        );

        var newPrincipal = new ClaimsPrincipal([.. otherIdentities, dsiIdentity]);
        await this.SignInProxyAsync(context, newPrincipal);
    }
}
