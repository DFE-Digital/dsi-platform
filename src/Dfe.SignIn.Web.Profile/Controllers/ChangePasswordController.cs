using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their password.
/// </summary>
[Authorize(Policy = PolicyNames.CanChangeOwnPassword)]
[Route("/change-password")]
public sealed partial class ChangePasswordController(
    IInteractionDispatcher interaction,
    ISelectAssociatedAccountHelper selectAssociatedAccountHelper
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await selectAssociatedAccountHelper.AuthenticateAssociatedAccount(
                this, ["https://graph.microsoft.com/.default"], SelectAssociatedReturnLocation.ChangePassword);
            if (actionResult is not null) {
                return actionResult;
            }
        }

        // Ensure that any prior inputs are cleared (eg. on failed form submission).
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.CurrentPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.NewPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.ConfirmNewPasswordInput), null, "");

        return this.View("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        ChangePasswordViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        GraphAccessToken? graphAccessToken = null;
        if (userProfileFeature.IsEntra) {
            graphAccessToken = await selectAssociatedAccountHelper.CreateAccessTokenForAssociatedAccount(
                this, ["https://graph.microsoft.com/.default"]);
        }

        await this.MapInteractionRequest<SelfChangePasswordRequest>(viewModel)
            .Use(request => request with {
                UserId = userProfileFeature.UserId,
                GraphAccessToken = graphAccessToken,
            })
            .InvokeAsync(interaction.DispatchAsync, cancellationToken);

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public IActionResult PostCancel()
    {
        this.SetFlashNotification(
            heading: "Password change cancelled",
            message: "As you did not complete the password change process, your password change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
