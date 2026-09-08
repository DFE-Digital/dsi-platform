using System.Net;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their email address.
/// </summary>
[Authorize(Policy = "CanChangeOwnEmailAddress")]
[Route("/change-email")]
public sealed class ChangeEmailController(
    IOptionsMonitor<ApplicationOidcOptions> oidcOptionsAccessor,
    IUsersApiClient usersApiClient,
    IValidator<ChangeEmailViewModel> changeEmailValidator,
    IValidator<VerificationCodeViewModel> verificationCodeValidator,
    //TODO: Review and remove dependency on IConfiguration
    IConfiguration configuration,
    ILogger<ChangeEmailController> logger
) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return this.View("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        [FromQuery] bool? resend,
        ChangeEmailViewModel viewModel)
    {
        var validationResult = await changeEmailValidator.ValidateAsync(viewModel);
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.View("Index");
        }

        var emailValidationEnabled = configuration.GetValue<bool>("EmailValidation");
        if (emailValidationEnabled) {
            var blockedResponse = await usersApiClient.CheckIfEmailAddressIsBlocked(new CheckIsBlockedEmailAddressRequest {
                EmailAddress = viewModel.EmailAddressInput
            });

            if (blockedResponse.IsBlocked) {
                this.ModelState.AddModelError(
                    nameof(viewModel.EmailAddressInput),
                    "This email address is not valid for this service. Generic email names (for example, headmaster@, admin@) and domains (for example, @yahoo.co.uk, @gmail.com) compromise security. Enter an email address that is associated with your organisation.");
                return this.View("Index");
            }
        }

        var request = new InitiateChangeEmailAddressRequest(
            oidcOptionsAccessor.CurrentValue.ClientId,
            viewModel.EmailAddressInput,
            true
        );

        var response = await usersApiClient.InitiateChangeEmailAddress(this.User.GetUserId(), request);
        if (response.IsSuccessStatusCode) {
            if (resend == true) {
                this.SetFlashSuccess(
                    heading: "Verification code resent",
                    message: $"""
                        We have sent an account verification email to {viewModel.EmailAddressInput}.
                        If the email address you provided is valid you will receive an email containing a verification code.
                        """
                );
            }

            this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = false;
            return this.RedirectToAction(nameof(VerificationCode));
        }

        if (response.StatusCode == HttpStatusCode.BadRequest) {
            await response.TryAddProblemDetailsToModelStateAsync(
                this.ModelState,
                ChangeEmailViewModel.RequestPropertyMap,
                fallbackField: nameof(ChangeEmailViewModel.EmailAddressInput));

            return this.View("Index");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests) {
            var detail = await response.GetDetailAsync();
            var errorMessage = !string.IsNullOrWhiteSpace(detail)
                ? detail
                : "For security reasons, the maximum number of verification code requests has been reached. Please try again later.";

            this.SetFlashNotification(
                heading: "Verification code limit reached",
                message: errorMessage
            );

            this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = true;
            return this.RedirectToAction(nameof(VerificationCode));
        }

        logger.LogError("Failed to initiate change email address for user {UserId}. StatusCode: {StatusCode}", this.User.GetUserId(), response.StatusCode);
        return this.ErrorView("ErrorUpdateEmailAddress");
    }

    [HttpGet("verify")]
    public Task<IActionResult> VerificationCode()
    {
        return this.VerificationCodeAnonymous(this.User.GetUserId());
    }

    [AllowAnonymous]
    [HttpGet("{userId}/verify")]
    public async Task<IActionResult> VerificationCodeAnonymous(
        [FromRoute] Guid userId)
    {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        return await this.RenderVerificationCodeViewAsync(userId);
    }

    [AllowAnonymous]
    [HttpPost("{userId}/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostVerificationCode(
        [FromRoute] Guid userId,
        VerificationCodeViewModel viewModel)
    {
        var validationResult = await verificationCodeValidator.ValidateAsync(viewModel);
        if (!validationResult.IsValid) {
            return await this.RenderVerificationCodeViewAsync(userId);
        }

        var request = new ConfirmChangeEmailAddressRequest {
            VerificationCode = viewModel.VerificationCodeInput!,
        };

        var response = await usersApiClient.ConfirmChangeEmailAddress(userId, request);
        if (response.IsSuccessStatusCode) {
            if (response.Content?.HasWarning(ChangeEmailWarnings.EntraMfaSyncFailed) == true) {
                logger.LogError("Partially failed to change email address for user {UserId}: Entra MFA sync failed.", userId);
                return this.ErrorView("ErrorUpdateAuthenticationMethod");
            }

            return this.RedirectToAction(nameof(Complete));
        }

        if (response.StatusCode == HttpStatusCode.BadRequest) {
            if (await response.IsProblemType(ChangeEmailErrors.NoPendingRequest)) {
                return this.RedirectToAction(nameof(Index));
            }

            await response.TryAddProblemDetailsToModelStateAsync(
                this.ModelState,
                VerificationCodeViewModel.RequestPropertyMap,
                fallbackField: nameof(VerificationCodeViewModel.VerificationCodeInput));

            return await this.RenderVerificationCodeViewAsync(userId);
        }

        logger.LogError("Failed to change email address for user {UserId}. StatusCode: {StatusCode}", userId, response.StatusCode);
        return this.ErrorView("ErrorUpdateEmailAddress");
    }

    [AllowAnonymous]
    [HttpGet("complete")]
    public IActionResult Complete()
    {
        return this.View("Complete");
    }

    [AllowAnonymous]
    [HttpPost("complete")]
    [ValidateAntiForgeryToken]
    public IActionResult PostComplete()
    {
        this.SetFlashSuccess(
            heading: "Email address updated successfully",
            message: "Please allow up to 10 minutes for the changes to take effect before signing back into your account."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostCancel()
    {
        await usersApiClient.CancelChangeEmailAddress(this.User.GetUserId());

        this.SetFlashNotification(
            heading: "Email change cancelled",
            message: "As you did not complete the email change process, your email change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<IActionResult> RenderVerificationCodeViewAsync(Guid userId)
    {
        var pendingChange = await this.GetPendingChangeEmailAddress(userId);
        if (pendingChange is null) {
            return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
        }

        this.ModelState.SetModelValue(nameof(VerificationCodeViewModel.VerificationCodeInput), null, "");

        return this.View("VerificationCode", new VerificationCodeViewModel {
            UserId = userId,
            NewEmailAddress = pendingChange.NewEmailAddress,
        });
    }

    private async Task<GetPendingChangeEmailResponse?> GetPendingChangeEmailAddress(Guid userId)
    {
        var response = await usersApiClient.GetPendingChangeEmail(userId);
        if (response.IsSuccessStatusCode) {
            return response.Content;
        }

        if (response.StatusCode != HttpStatusCode.NotFound) {
            logger.LogWarning("Unexpected status code {StatusCode} when retrieving pending change email for user {UserId}", response.StatusCode, userId);
        }

        return null;
    }
}
