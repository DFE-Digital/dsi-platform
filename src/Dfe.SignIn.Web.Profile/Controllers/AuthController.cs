using Dfe.SignIn.Web.Profile.Services;
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
    IHybridAuthentication hybridAuthentication
) : Controller
{
    [Authorize]
    [HttpGet("signout")]
    public new IActionResult SignOut() => this.SignOut(
        OpenIdConnectDefaults.AuthenticationScheme,
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    [Authorize]
    [HttpGet("/auth/callback")]
    public async Task<IActionResult> HybridCallback()
    {
        var redirectUri = await hybridAuthentication.ProcessCallbackAsync();
        return redirectUri is not null
            ? this.Redirect(redirectUri.ToString())
            : this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
