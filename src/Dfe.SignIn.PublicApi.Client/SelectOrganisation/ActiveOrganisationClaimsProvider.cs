using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// A service that gets or sets the active organisation of a user by adding an
/// organisation identity to the <see cref="ClaimsPrincipal"/>.
/// </summary>
public class ActiveOrganisationClaimsProvider : IActiveOrganisationProvider
{
    /// <inheritdoc/>
    public virtual async Task SetActiveOrganisationAsync(IHttpContext context, OrganisationDetails? organisation)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var otherIdentities = context.User.Identities
            .Where(identity => identity.AuthenticationType != PublicApiConstants.AuthenticationType);

        var identity = new ClaimsIdentity(PublicApiConstants.AuthenticationType);

        string organisationJson = DsiClaimExtensions.SerializeDsiOrganisation(organisation);

        identity.AddClaims([
            new Claim(DsiClaimTypes.SessionId, context.User.GetSessionId()),
            new Claim(DsiClaimTypes.Organisation, organisationJson, JsonClaimValueTypes.Json),
        ]);

        var newPrincipal = new ClaimsPrincipal([.. otherIdentities, identity]);
        await context.SignInAsync(newPrincipal);
    }

    /// <inheritdoc/>
    public virtual Task<ActiveOrganisationState?> GetActiveOrganisationStateAsync(IHttpContext context)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var identity = context.User.GetDsiOrganisationIdentity();

        if (identity is null) {
            return Task.FromResult<ActiveOrganisationState?>(null);
        }

        return Task.FromResult<ActiveOrganisationState?>(
            new ActiveOrganisationState {
                Organisation = identity?.DeserializeDsiOrganisation(),
            }
        );
    }
}
