using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their password.
/// </summary>
[Route("/change-password")]
public sealed partial class ChangePasswordController(
    IInteractionDispatcher interaction,
    IHybridAuthentication hybridAuthentication
) : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (this.HttpContext.Session.GetInt32(SessionKeys.IsInternalUser) == 1) {
            return this.NotFound();
        }

        Guid userId = this.User.GetUserId();

        var profileResponse = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        if (profileResponse.IsEntra) {
            hybridAuthentication.VerifyAccessToken(
                callbackUri: this.Url.Action("HybridCallback", "Auth", null, this.Request.Scheme)!,
                loginHint: profileResponse.EmailAddress,
                out AccessToken? _,
                out Task<Uri>? getAuthUrlTask
            );

            if (getAuthUrlTask is not null) {
                return this.Redirect((await getAuthUrlTask).ToString());
            }
        }

        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.CurrentPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.NewPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.ConfirmNewPasswordInput), null, "");

        return this.View("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostIndex(ChangePasswordViewModel viewModel)
    {
        Guid userId = this.User.GetUserId();
        GraphAccessToken? graphAccessToken = null;

        var profileResponse = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        if (profileResponse.IsEntra) {
            var hybridAccessToken = hybridAuthentication.GetAccessTokenFromSession();
            if (hybridAccessToken is null || (hybridAccessToken.Value.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes < 1) {
                this.SetFlashNotification(
                    heading: "Password change cancelled",
                    message: "Your session has expired, your password change has been cancelled. Please try again."
                );
                return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
            }

            graphAccessToken = new() {
                Token = hybridAccessToken.Value.Token,
                ExpiresOn = hybridAccessToken.Value.ExpiresOn,
            };
        }

        await this.MapInteractionRequest<SelfChangePasswordRequest>(viewModel)
            .Use(request => request with {
                UserId = this.User.GetUserId(),
                GraphAccessToken = graphAccessToken,
            })
            .InvokeAsync(interaction.DispatchAsync);

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [Authorize]
    [HttpPost("cancel")]
    public IActionResult PostCancel()
    {
        this.SetFlashNotification(
            heading: "Password change cancelled",
            message: "As you did not complete the password change process, your password change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
