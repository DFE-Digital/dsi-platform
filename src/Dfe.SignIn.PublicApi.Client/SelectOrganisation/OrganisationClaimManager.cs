using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
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
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the user is not currently authenticated.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task UpdateOrganisationClaimAsync(
        IHttpContext context,
        string organisationJson,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Concrete implementation of <see cref="IOrganisationClaimManager"/>.
/// </summary>
internal sealed class OrganisationClaimManager(
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor
) : IOrganisationClaimManager
{
    /// <inheritdoc/>
    public async Task UpdateOrganisationClaimAsync(
        IHttpContext context,
        string organisationJson,
        CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));
        ExceptionHelpers.ThrowIfArgumentNull(organisationJson, nameof(organisationJson));

        var options = optionsAccessor.Value;

        var otherIdentities = context.User.Identities
            .Where(identity => identity.AuthenticationType != PublicApiConstants.AuthenticationType);

        var dsiIdentity = new ClaimsIdentity(PublicApiConstants.AuthenticationType);

        if (options.UpdateClaimsIdentity is not null) {
            dsiIdentity = await options.UpdateClaimsIdentity.Invoke(dsiIdentity);
        }

        dsiIdentity.AddClaims([
            new Claim(DsiClaimTypes.SessionId, context.User.GetSessionId()),
            new Claim(DsiClaimTypes.Organisation, organisationJson, JsonClaimValueTypes.Json),
        ]);

        var newPrincipal = new ClaimsPrincipal([.. otherIdentities, dsiIdentity]);
        await context.SignInAsync(newPrincipal);
    }
}
