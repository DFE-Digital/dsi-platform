using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller for handling user authentication.
/// </summary>
[Route("/")]
public sealed class AuthController(
    IHybridAuthentication hybridAuthentication,
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet("signout")]
    public new Task<IActionResult> SignOut()
    {
        return this.SignOutHelper();
    }

    [AllowAnonymous]
    [HttpGet("/session-timeout")]
    public async Task<IActionResult> Timeout()
    {
        if (this.User.Identity?.IsAuthenticated == true) {
            return await this.SignOutHelper(this.Url.Action(nameof(Timeout)));
        }

        return this.View("Auth/SessionTimeout");
    }

    [Authorize]
    [HttpGet("/auth/callback")]
    public async Task<IActionResult> HybridCallback()
    {
        var redirectUri = await hybridAuthentication.ProcessCallbackAsync();
        return redirectUri is not null
            ? this.Redirect(redirectUri.ToString())
            : this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<IActionResult> SignOutHelper(string? redirectUri = null)
    {
        if (redirectUri is not null && this.User.Identity?.IsAuthenticated != true) {
            return this.Redirect(redirectUri);
        }

        Guid? userId = this.User.GetUserId();

        var result = this.SignOut(
            new AuthenticationProperties { RedirectUri = redirectUri },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        if (this.User.Identity?.IsAuthenticated == true) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.SignOut,
                Message = "User logged out",
                UserId = userId,
            }, CancellationToken.None);
        }

        return result;
    }
}
