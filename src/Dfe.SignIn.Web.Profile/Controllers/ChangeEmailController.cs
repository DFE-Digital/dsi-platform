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
        bool hideResendVerificationBanner = false;

        var validationResult = await changeEmailValidator.ValidateAsync(viewModel);
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.View("Index");
        }
        try {
            var emailValidationEnabled = configuration.GetValue<bool>("EmailValidation");

            if (emailValidationEnabled) {
                var blockedResponse = await usersApiClient.CheckIfEmailAddressIsBlocked(new CheckIsBlockedEmailAddressRequest {
                    EmailAddress = viewModel.EmailAddressInput
                });

                if (blockedResponse.IsBlocked) {
                    this.ModelState.AddModelError(nameof(viewModel.EmailAddressInput), "This email address is not valid for this service. Generic email names (for example, headmaster@, admin@) and domains (for example, @yahoo.co.uk, @gmail.com) compromise security. Enter an email address that is associated with your organisation.");
                    return this.View("Index");
                }
            }
            var request = new InitiateChangeEmailAddressRequest(
                oidcOptionsAccessor.CurrentValue.ClientId,
                viewModel.EmailAddressInput,
                true
            );

            await usersApiClient.InitiateChangeEmailAddress(this.User.GetUserId(), request);

            if (resend == true) {
                this.SetFlashSuccess(
                    heading: "Verification code resent",
                    message: $"""
                        We have sent an account verification email to {viewModel.EmailAddressInput}.
                        If the email address you provided is valid you will receive an email containing a verification code.
                        """
                );
            }
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest) {
            var message = ex.Content?.ToString() ?? "We couldn't change your email address right now. Please try again.";
            this.ModelState.AddModelError(nameof(ChangeEmailViewModel.EmailAddressInput), message);
            return this.View("Index");
        }
        catch (Refit.ValidationApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests) {
            var errorMessage = !string.IsNullOrWhiteSpace(ex.Content?.Detail)
                ? ex.Content.Detail
                : "For security reasons, the maximum number of verification code requests has been reached. Please try again later.";

            this.SetFlashNotification(
                heading: "Verification code limit reached",
                message: errorMessage
            );

            hideResendVerificationBanner = true;
        }

        this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = hideResendVerificationBanner;
        return this.RedirectToAction(nameof(VerificationCode));
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

        return await this.VerificationCodeHelper(userId);
    }

    private async Task<IActionResult> VerificationCodeHelper(Guid userId)
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

    [AllowAnonymous]
    [HttpPost("{userId}/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostVerificationCode(
        [FromRoute] Guid userId,
        VerificationCodeViewModel viewModel)
    {
        var validationResult = await verificationCodeValidator.ValidateAsync(viewModel);
        if (!validationResult.IsValid) {
            return await this.VerificationCodeHelper(userId);
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

            return await this.VerificationCodeHelper(userId);
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

    private async Task<GetPendingChangeEmailResponse?> GetPendingChangeEmailAddress(Guid userId)
    {
        try {
            return await usersApiClient.GetPendingChangeEmail(userId);
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
            return null;
        }
    }
}
