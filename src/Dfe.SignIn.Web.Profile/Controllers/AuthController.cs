using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller for handling user authentication.
/// </summary>
[Route("/")]
public sealed class AuthController : Controller
{
    [Authorize]
    [HttpGet("signout")]
    public new IActionResult SignOut() => this.SignOut(
        OpenIdConnectDefaults.AuthenticationScheme,
        CookieAuthenticationDefaults.AuthenticationScheme
    );
}
