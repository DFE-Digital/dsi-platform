using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// Represents a service for authenticating the external account that is
/// associated with the user's DfE Sign-In account.
/// </summary>
public interface ISelectAssociatedAccountHelper
{
    /// <summary>
    /// Authenticates the external account that is associated with the user's
    /// DfE Sign-In account.
    /// </summary>
    /// <param name="controller">The current controller.</param>
    /// <param name="scopes">The array of required scopes.</param>
    /// <param name="redirectUri">The URI where the user will be redirected to
    /// upon authenticating with the external identity provider.</param>
    /// <param name="force">Indicates if the user should be forced to authenticate
    /// with the external identity provider. This is useful when the user has
    /// multiple external accounts and is currently using one that is not associated
    /// with their DfE Sign-In account.</param>
    /// <returns>
    ///   <para>A value of null when the user is authenticated.</para>
    ///   <para>- or -</para>
    ///   <para>The action result that the current controller should return to
    ///   guide the user through the external authentication process.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="scopes"/> is null.</para>
    /// </exception>
    Task<IActionResult?> AuthenticateAssociatedAccount(
        Controller controller, string[] scopes, string? redirectUri, bool force = false);

    /// <summary>
    /// Creates an access token for the external account that is associated with
    /// the user's DfE Sign-In account.
    /// </summary>
    /// <param name="controller">The current controller.</param>
    /// <param name="scopes">The array of required scopes.</param>
    /// <returns>
    ///   <para>A value of null if the user is not associated with an external account.</para>
    ///   <para>- or -</para>
    ///   <para>A <see cref="GraphAccessToken"/> that can be used to authorize
    ///   requests to the Graph API.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="scopes"/> is null.</para>
    /// </exception>
    /// <exception cref="BadHttpRequestException">
    ///   <para>If the user is not authenticated with an associated account.</para>
    ///   <para>- or -</para>
    ///   <para>If the associated claims principal is missing the required
    ///   'dsi_user_id' claim.</para>
    ///   <para>- or -</para>
    ///   <para>If the external account is not actually associated with the
    ///   user's DfE Sign-In account.</para>
    /// </exception>
    Task<GraphAccessToken?> CreateAccessTokenForAssociatedAccount(
        Controller controller, string[] scopes);
}

/// <summary>
/// A service for authenticating the external account that is associated with
/// the user's DfE Sign-In account.
/// </summary>
public sealed class SelectAssociatedAccountHelper(
    ITokenAcquisition tokenAcquisition,
    TimeProvider timeProvider
) : ISelectAssociatedAccountHelper
{
    /// <inheritdoc/>
    public async Task<IActionResult?> AuthenticateAssociatedAccount(
        Controller controller, string[] scopes, string? redirectUri, bool force = false)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNull(scopes, nameof(scopes));

        var userProfileFeature = controller.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        IActionResult Challenge()
        {
            return controller.Challenge(new AuthenticationProperties {
                IsPersistent = true,
                RedirectUri = redirectUri,
                Parameters = {
                    { "login_hint", userProfileFeature.EmailAddress },
                },
            }, ExternalAuthConstants.OpenIdConnectSchemeName);
        }

        try {
            var authenticateResult = await controller.HttpContext.AuthenticateAsync(ExternalAuthConstants.OpenIdConnectSchemeName);
            if (!authenticateResult.Succeeded) {
                return Challenge();
            }

            var dsiUserIdClaim = authenticateResult.Principal?.FindFirst(DsiClaimTypes.UserId);

            if (!Guid.TryParse(dsiUserIdClaim?.Value, out Guid dsiUserId) || force) {
                return Challenge();
            }

            if (dsiUserId != userProfileFeature.UserId) {
                return controller.View("SelectAssociatedAccount", new SelectAssociatedAccountViewModel {
                    RedirectUri = redirectUri ?? controller.Url.Action(nameof(HomeController.Index), nameof(HomeController)),
                });
            }

            await tokenAcquisition.GetAccessTokenForUserAsync(
                scopes,
                authenticationScheme: ExternalAuthConstants.OpenIdConnectSchemeName,
                user: authenticateResult.Principal
            );

            return null;
        }
        catch (Exception) {
            return Challenge();
        }
    }

    /// <inheritdoc/>
    public async Task<GraphAccessToken?> CreateAccessTokenForAssociatedAccount(
        Controller controller, string[] scopes)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNull(scopes, nameof(scopes));

        var userProfileFeature = controller.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (!userProfileFeature.IsEntra) {
            return null;
        }

        var authenticateResult = await controller.HttpContext.AuthenticateAsync(ExternalAuthConstants.OpenIdConnectSchemeName);
        if (!authenticateResult.Succeeded) {
            throw new BadHttpRequestException("Not authenticated with associated account.", StatusCodes.Status403Forbidden);
        }

        var dsiUserIdClaim = authenticateResult.Principal?.FindFirst("dsi_user_id");
        if (!Guid.TryParse(dsiUserIdClaim?.Value, out Guid dsiUserId)) {
            throw new BadHttpRequestException("Missing required claim 'dsi_user_id'.", StatusCodes.Status403Forbidden);
        }

        if (dsiUserId != userProfileFeature.UserId) {
            throw new BadHttpRequestException("Account mismatch.", StatusCodes.Status403Forbidden);
        }

        string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(
            scopes,
            authenticationScheme: ExternalAuthConstants.OpenIdConnectSchemeName,
            user: authenticateResult.Principal
        );

        return new() {
            Token = accessToken,
            ExpiresOn = timeProvider.GetUtcNow().AddMinutes(1),
        };
    }
}
