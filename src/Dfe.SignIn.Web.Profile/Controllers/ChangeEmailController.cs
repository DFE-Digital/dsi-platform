using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their email address.
/// </summary>
public sealed class ChangeEmailController(
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet("/change-email")]
    public async Task<IActionResult> Index()
    {
        return this.View();
    }

    [Authorize]
    public async Task<IActionResult> PostIndex(ChangeEmailViewModel postModel)
    {
        this.TempData["EmailAddressInput"] = postModel.EmailAddressInput;

        return this.RedirectToAction("VerificationCode");
    }

    [Authorize]
    [HttpGet("/change-email/verify")]
    public async Task<IActionResult> VerificationCode()
    {
        Guid userId = this.User.GetUserId();

        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest { UserId = userId }
        ).To<GetPendingChangeEmailAddressResponse>();

        if (pendingChangeEmailAddressResponse.PendingChangeEmailAddress is null) {
            return this.RedirectToAction("Index", "Home");
        }

        return this.View("VerificationCode", new VerificationCodeViewModel {
            NewEmailAddress = (this.TempData["EmailAddressInput"] as string)!,
        });
    }

    [Authorize]
    public async Task<IActionResult> PostVerificationCode()
    {
        return this.RedirectToAction("Complete");
    }

    [Authorize]
    public async Task<IActionResult> PostResendVerificationCode()
    {
        return this.RedirectToAction("VerificationCode");
    }

    [Authorize]
    [HttpGet("/change-email/complete")]
    public async Task<IActionResult> Complete()
    {
        return this.View();
    }

    [Authorize]
    public async Task<IActionResult> PostComplete()
    {
        this.SetFlashNotification(
            heading: "Email address updated successfully",
            message: "Please allow up to 10 minutes for the changes to take effect before signing back into your account."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> PostCancel()
    {
        Guid userId = this.User.GetUserId();

        await interaction.DispatchAsync(
            new CancelPendingChangeEmailAddressRequest { UserId = userId }
        );

        this.SetFlashNotification(
            heading: "Email change cancelled",
            message: "As you did not complete the email change process, your email change has been cancelled."
        );

        return this.RedirectToAction("Index", "Home");
    }
}
