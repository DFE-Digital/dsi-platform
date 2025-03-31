using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Internal;
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
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the user is not currently authenticated.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task UpdateOrganisationClaimAsync(
        HttpContext context,
        string organisationJson,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Concrete implementation of <see cref="IOrganisationClaimManager"/>.
/// </summary>
internal sealed class OrganisationClaimManager(
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor,
    IInteractor<GetUserAccessToServiceRequest, GetUserAccessToServiceResponse> getUserAccessToService
) : IOrganisationClaimManager
{
    // Proxy for 'SignInAsync' function since `HttpContext.SignInAsync` is
    // difficult to mock.
    internal Func<HttpContext, ClaimsPrincipal, Task> SignInProxyAsync { get; set; }
        = (context, principal) => context.SignInAsync(principal);

    /// <inheritdoc/>
    public async Task UpdateOrganisationClaimAsync(
        HttpContext context,
        string organisationJson,
        CancellationToken cancellationToken = default)
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
            new Claim(DsiClaimTypes.Organisation, organisationJson, JsonClaimValueTypes.Json)
        );

        await this.FetchRolesFromPublicApi(
            context.User.GetDsiUserId(),
            dsiIdentity,
            options.FetchRoleClaimsFlags,
            cancellationToken
        );

        var newPrincipal = new ClaimsPrincipal([.. otherIdentities, dsiIdentity]);
        await this.SignInProxyAsync(context, newPrincipal);
    }

    private async Task FetchRolesFromPublicApi(
        Guid userId,
        ClaimsIdentity identity,
        FetchRoleClaimsFlag flags,
        CancellationToken cancellationToken = default)
    {
        if (flags == FetchRoleClaimsFlag.None) {
            return;
        }

        var organisation = identity.GetDsiOrganisationInternal();
        if (organisation is not null) {
            var details = await getUserAccessToService.InvokeAsync(new() {
                UserId = userId,
                OrganisationId = organisation.Id,
            }, cancellationToken);
            foreach (var role in details.Roles) {
                if (role.Status.Id != 1) {
                    continue;
                }

                if ((flags & FetchRoleClaimsFlag.RoleId) != 0) {
                    identity.AddClaim(
                        new Claim(DsiClaimTypes.RoleId, role.Id.ToString(), ClaimValueTypes.String)
                    );
                }
                if ((flags & FetchRoleClaimsFlag.RoleName) != 0) {
                    identity.AddClaim(
                        new Claim(DsiClaimTypes.RoleName, role.Name, ClaimValueTypes.String)
                    );
                }
                if ((flags & FetchRoleClaimsFlag.RoleCode) != 0) {
                    identity.AddClaim(
                        new Claim(DsiClaimTypes.RoleCode, role.Code, ClaimValueTypes.String)
                    );
                }
                if ((flags & FetchRoleClaimsFlag.RoleNumericId) != 0) {
                    identity.AddClaim(
                        new Claim(DsiClaimTypes.RoleNumericId, role.NumericId, ClaimValueTypes.String)
                    );
                }
            }
        }
    }
}
