using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Core.Interfaces.Graph;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their password.
/// </summary>
[Microsoft.AspNetCore.Authorization.Authorize(Policy = PolicyNames.CanChangeOwnPassword)]
[Route("/change-password")]
public sealed partial class ChangePasswordController(
    IUsersApiClient usersApiClient,
    IGraphApiChangeUserPassword graphApiChangeUserPassword,
    IValidator<ChangePasswordViewModel> changePasswordValidator,
    ISelectAssociatedAccountHelper selectAssociatedAccountHelper,
    ILogger<ChangePasswordController> logger
) : Controller
{
    private const string GraphApiEndpoint = "https://graph.microsoft.com/.default";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await selectAssociatedAccountHelper.AuthenticateAssociatedAccount(
                this, [GraphApiEndpoint], SelectAssociatedReturnLocation.ChangePassword);
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
        var validationResult = await changePasswordValidator.ValidateAsync(viewModel);

        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return await this.Index();
        }

        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            try {
                GraphAccessToken? graphAccessToken = await selectAssociatedAccountHelper.CreateAccessTokenForAssociatedAccount(
                    this, [GraphApiEndpoint]) ?? throw new Exception("Provided graph token for user is null");

                await graphApiChangeUserPassword.ChangePassword(
                    viewModel.CurrentPasswordInput!,
                    viewModel.NewPasswordInput!,
                    graphAccessToken);
            }
            catch (ValidationException ex) {
                foreach (var error in ex.Errors) {
                    this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return await this.Index();
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred while changing the user's password.");
                this.ModelState.AddModelError(string.Empty, "We couldn't change your password right now. Please try again.");
                return await this.Index();
            }
        } else {
            try {
                var request = new ChangePasswordRequest {
                    UserId = userProfileFeature.UserId,
                    CurrentPassword = viewModel.CurrentPasswordInput!,
                    NewPassword = viewModel.NewPasswordInput!,
                    ConfirmNewPassword = viewModel.ConfirmNewPasswordInput!,
                };
                await usersApiClient.ChangePassword(userProfileFeature.UserId, request);
            }
            catch (ApiException ex) when (ex.Content is not null) {
                var problemDetails = await ex.GetContentAsAsync<ValidationProblemDetails>();
                if (problemDetails?.Errors != null) {
                    foreach (var error in problemDetails.Errors) {
                        // Map specific error messages to the view model properties
                        string propertyName = error.Key switch {
                            nameof(ChangePasswordRequest.CurrentPassword) => nameof(ChangePasswordViewModel.CurrentPasswordInput),
                            nameof(ChangePasswordRequest.NewPassword) => nameof(ChangePasswordViewModel.NewPasswordInput),
                            _ => string.Empty
                        };
                        foreach (var message in error.Value) {
                            this.ModelState.AddModelError(propertyName, message);
                        }
                    }
                    return await this.Index();
                }
                logger.LogError(ex, "An API error occurred while changing the user's password.");
                this.ModelState.AddModelError(string.Empty, "We couldn't change your password right now. Please try again.");
                return await this.Index();
            }
            catch (Exception ex) {
                logger.LogError(ex, "An error occurred while changing the user's password.");
                this.ModelState.AddModelError(string.Empty, "We couldn't change your password right now. Please try again.");
                return await this.Index();
            }
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
