using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
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
public sealed class AuthController(IInteractionDispatcher interaction) : Controller
{
    [Authorize]
    [HttpGet("signout")]
    public new Task<IActionResult> SignOut()
    {
        return this.SignOutHelper();
    }

    [AllowAnonymous]
    [HttpGet("/session/end-timeout")]
    public Task<IActionResult> EndSessionTimeout()
    {
        return this.SignOutHelper(this.Url.Action(nameof(Timeout)));
    }

    [AllowAnonymous]
    [HttpGet("/session-timeout")]
    public async Task<IActionResult> Timeout()
    {
        if (this.User.Identity?.IsAuthenticated == true) {
            return await this.SignOutHelper("/");
        }

        return this.View("Auth/SessionTimeout");
    }

    private async Task<IActionResult> SignOutHelper(string? redirectUri = null)
    {
        var result = this.SignOut(
            new AuthenticationProperties { RedirectUri = redirectUri },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        if (this.User.Identity?.IsAuthenticated == true) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.SignOut,
                Message = "User signing out",
                UserId = this.User.GetUserId(),
            }, CancellationToken.None);
        }

        return result;
    }
}
