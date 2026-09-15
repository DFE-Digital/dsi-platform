using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;
using Dfe.SignIn.Gateways.Entra.ChangePassword;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services.AssociatedAccountAuth;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Dfe.SignIn.WebFramework.Mvc.Validation;
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
    IUsersApiClient usersApiClient,
    IEntraChangePasswordService entraChangePasswordService,
    IAssociatedAccountAuthService associatedAccountAuthService,
    ILogger<ChangePasswordController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await associatedAccountAuthService.AuthenticateAssociatedAccount(
                this, [AssociatedAccountConstants.DefaultGraphScope], SelectAssociatedReturnLocation.ChangePassword);
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
    public async Task<IActionResult> PostIndex(ChangePasswordViewModel viewModel)
    {
        var validationResult = await viewModel.ValidateAsync<ChangePasswordViewModelValidator, ChangePasswordViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return await this.Index();
        }

        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        var success = userProfileFeature.IsEntra
            ? await this.TryChangeEntraPasswordAsync(viewModel)
            : await this.TryChangeLocalPasswordAsync(viewModel, userProfileFeature.UserId);

        if (!success) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<bool> TryChangeEntraPasswordAsync(ChangePasswordViewModel viewModel)
    {
        try {
            var graphAccessToken = await associatedAccountAuthService.CreateAccessTokenForAssociatedAccount(
                this, [AssociatedAccountConstants.DefaultGraphScope]) ?? throw new InvalidOperationException("Provided graph token for user is null");

            var result = await entraChangePasswordService.ChangePasswordAsync(
                viewModel.CurrentPasswordInput!,
                viewModel.NewPasswordInput!,
                graphAccessToken);

            if (result.IsSuccess) {
                return true;
            }

            if (result.Error.Code == EntraPasswordErrors.InvalidCurrentPasswordCode) {
                this.ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPasswordInput), result.Error.Description);
                return false;
            }

            if (result.Error.Code == EntraPasswordErrors.PasswordPolicyViolationCode) {
                this.ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPasswordInput), result.Error.Description);
                return false;
            }

            this.ModelState.AddModelError(string.Empty, "We couldn't change your password right now. Please try again.");
            return false;
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred while changing the user's password.");
            this.ModelState.AddModelError(string.Empty, "We couldn't change your password right now. Please try again.");
            return false;
        }
    }

    private async Task<bool> TryChangeLocalPasswordAsync(ChangePasswordViewModel viewModel, Guid userId)
    {
        var request = new ChangePasswordRequest {
            CurrentPassword = viewModel.CurrentPasswordInput!,
            NewPassword = viewModel.NewPasswordInput!,
            ConfirmNewPassword = viewModel.ConfirmNewPasswordInput!,
        };

        var response = await usersApiClient.ChangePassword(userId, request);

        if (response.IsSuccessStatusCode) {
            return true;
        }

        await response.TryAddProblemDetailsToModelStateAsync(this.ModelState, ChangePasswordViewModel.RequestPropertyMap);
        logger.LogWarning("Failed to change password for user {UserId}. Response: {Response}", userId, response);
        return false;
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
