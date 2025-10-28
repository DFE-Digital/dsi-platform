using System.ComponentModel.DataAnnotations;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their password.
/// </summary>
public sealed partial class ChangePasswordController(
    IInteractionValidator interactionValidator,
    IInteractionDispatcher interaction,
    IHybridAuthentication hybridAuthentication,
    IPersonalGraphServiceFactory graphServiceFactory
) : Controller
{
    [Authorize]
    [HttpGet("/change-password")]
    public async Task<IActionResult> Index()
    {
        Guid userId = this.User.GetUserId();

        var profileResponse = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        if (profileResponse.IsEntra) {
            hybridAuthentication.VerifyAccessToken(
                callbackUri: this.Url.Action("HybridCallback", "Auth", null, this.Request.Scheme)!,
                loginHint: profileResponse.EmailAddress,
                out AccessToken? accessToken,
                out Task<Uri>? getAuthUrlTask
            );

            if (getAuthUrlTask is not null) {
                return this.Redirect((await getAuthUrlTask).ToString());
            }
        }

        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.CurrentPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.NewPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.ConfirmNewPasswordInput), null, "");

        return this.View("Index", new ChangePasswordViewModel {
            PostAction = profileResponse.IsEntra ? nameof(PostIndex) : nameof(PostIndexLegacy),
        });
    }

    [Authorize]
    [HttpPost("/change-password")]
    public async Task<IActionResult> PostIndex(ChangePasswordViewModel viewModel)
    {
        var request = new SelfChangePasswordRequest {
            UserId = this.User.GetUserId(),
            CurrentPassword = viewModel.CurrentPasswordInput!,
            NewPassword = viewModel.NewPasswordInput!,
            ConfirmNewPassword = viewModel.ConfirmNewPasswordInput!,
        };

        var validationResults = new List<ValidationResult>();
        interactionValidator.TryValidateRequest(request, validationResults);
        this.ModelState.AddFrom(validationResults, new() {
            [nameof(SelfChangePasswordRequest.CurrentPassword)] = nameof(ChangePasswordViewModel.CurrentPasswordInput),
            [nameof(SelfChangePasswordRequest.NewPassword)] = nameof(ChangePasswordViewModel.NewPasswordInput),
            [nameof(SelfChangePasswordRequest.ConfirmNewPassword)] = nameof(ChangePasswordViewModel.ConfirmNewPasswordInput),
        });

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        var hybridAccessToken = hybridAuthentication.GetAccessTokenFromSession();
        if (hybridAccessToken is null || (DateTimeOffset.UtcNow - hybridAccessToken.Value.ExpiresOn).TotalMinutes < 1) {
            this.SetFlashNotification(
                heading: "Password change cancelled",
                message: "Your session has expired, your password change has been cancelled. Please try again."
            );
            return this.RedirectToAction("Index", "Home");
        }

        var graphClient = graphServiceFactory.GetClient(hybridAccessToken.Value);
        await graphClient.Me.ChangePassword.PostAsync(new() {
            CurrentPassword = viewModel.CurrentPasswordInput,
            NewPassword = viewModel.NewPasswordInput,
        });

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost("/change-password/legacy")]
    public async Task<IActionResult> PostIndexLegacy(ChangePasswordViewModel viewModel)
    {
        try {
            await interaction.DispatchAsync(
                new SelfChangePasswordRequest {
                    UserId = this.User.GetUserId(),
                    CurrentPassword = viewModel.CurrentPasswordInput!,
                    NewPassword = viewModel.NewPasswordInput!,
                    ConfirmNewPassword = viewModel.ConfirmNewPasswordInput!,
                }
            );
        }
        catch (InvalidRequestException ex) {
            this.ModelState.AddFrom(ex.ValidationResults, new() {
                [nameof(SelfChangePasswordRequest.CurrentPassword)] = nameof(ChangePasswordViewModel.CurrentPasswordInput),
                [nameof(SelfChangePasswordRequest.NewPassword)] = nameof(ChangePasswordViewModel.NewPasswordInput),
                [nameof(SelfChangePasswordRequest.ConfirmNewPassword)] = nameof(ChangePasswordViewModel.ConfirmNewPasswordInput),
            });
            this.ModelState.ThrowIfNoErrorsRecorded(ex);
        }

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost("/change-password/cancel")]
    public async Task<IActionResult> PostCancel()
    {
        this.SetFlashNotification(
            heading: "Password change cancelled",
            message: "As you did not complete the password change process, your password change has been cancelled."
        );

        return this.RedirectToAction("Index", "Home");
    }
}
