using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their password.
/// </summary>
public sealed class ChangePasswordController : Controller
{
    [Authorize]
    [HttpGet("/change-password")]
    public async Task<IActionResult> Index()
    {
        return this.View();
    }

    [Authorize]
    public async Task<IActionResult> PostIndex()
    {
        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> PostCancel()
    {
        this.SetFlashNotification(
            heading: "Password change cancelled",
            message: "As you did not complete the password change process, your password change has been cancelled."
        );

        return this.RedirectToAction("Index", "Home");
    }
}
