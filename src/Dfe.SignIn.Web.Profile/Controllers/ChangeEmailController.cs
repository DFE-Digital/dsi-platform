using System.Net;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Features.Users.CheckIsBlockedEmail;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their email address.
/// </summary>
[Authorize(Policy = PolicyNames.CanChangeOwnEmailAddress)]
[Route("/change-email")]
public sealed class ChangeEmailController(
    IOptionsMonitor<ApplicationOidcOptions> oidcOptionsAccessor,
    IUsersApiClient usersApiClient,
    IOptions<EmailValidationSettings> EmailValidationSettings,
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
        ChangeEmailViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var validationResult = await viewModel.ValidateAsync<ChangeEmailViewModelValidator, ChangeEmailViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.View("Index");
        }

        try {
            return await this.InitiateEmailChangeAsync(viewModel, resend == true, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Failed to initiate change email address for user {UserId}.", this.User.GetUserId());
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }
    }

    private async Task<IActionResult> InitiateEmailChangeAsync(
        ChangeEmailViewModel viewModel, bool isResend, CancellationToken cancellationToken)
    {
        if (EmailValidationSettings.Value.Enabled) {
            var blockedResponse = await usersApiClient.CheckIfEmailAddressIsBlocked(new CheckIsBlockedEmailAddressRequest {
                EmailAddress = viewModel.EmailAddressInput!
            }, cancellationToken);

            if (!blockedResponse.IsSuccessStatusCode || blockedResponse.Content is null) {
                logger.LogError("Failed to check email blocklist. StatusCode: {StatusCode}", blockedResponse.StatusCode);
                return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
            }

            if (blockedResponse.Content.IsBlocked) {
                this.ModelState.AddModelError(
                    nameof(viewModel.EmailAddressInput),
                    "This email address is not valid for this service. Generic email names (for example, headmaster@, admin@) and domains (for example, @yahoo.co.uk, @gmail.com) compromise security. Enter an email address that is associated with your organisation.");
                return this.View("Index");
            }
        }

        var request = new InitiateChangeEmailAddressRequest(
            oidcOptionsAccessor.CurrentValue.ClientId,
            viewModel.EmailAddressInput!,
            true
        );

        var response = await usersApiClient.InitiateChangeEmailAddress(this.User.GetUserId(), request, cancellationToken);
        return await this.HandleInitiateChangeResponseAsync(response, viewModel.EmailAddressInput!, isResend);
    }

    private async Task<IActionResult> HandleInitiateChangeResponseAsync(
        Refit.IApiResponse response,
        string emailAddress,
        bool isResend)
    {
        if (response.IsSuccessStatusCode) {
            if (isResend) {
                this.SetFlashSuccess(
                    heading: "Verification code resent",
                    message: $"""
                        We have sent an account verification email to {emailAddress}.
                        If the email address you provided is valid you will receive an email containing a verification code.
                        """
                );
            }

            this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = false;
            return this.RedirectToAction(nameof(VerificationCode));
        }

        if (response.StatusCode == HttpStatusCode.BadRequest) {

            //ASP.NET Core's default TempData provider is cookie-based.
            //Data within it is encrypted, signed and sent back on each request.
            //It's needed to show the users 'new' email in the flash message. Without it
            //we have no way of knowing what the user tried to use. If we don't show
            //anything we leak the fact that the email is in use causing a information leak.
            this.TempData[VerificationCodeViewModel.NewEmailAddressTempDataKey] = emailAddress;

            return this.RedirectToAction(nameof(VerificationCode));
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

        logger.LogError(
            "Failed to initiate change email address for user {UserId}. StatusCode: {StatusCode}",
            this.User.GetUserId(),
            response.StatusCode);
        return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
    }

    [HttpGet("verify")]
    public Task<IActionResult> VerificationCode(CancellationToken cancellationToken)
    {
        return this.VerificationCodeAnonymous(this.User.GetUserId(), cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("{userId}/verify")]
    public async Task<IActionResult> VerificationCodeAnonymous(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        return await this.RenderVerificationCodeViewAsync(userId, cancellationToken);
    }

    [HttpPost("verify")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> PostVerificationCode(
        VerificationCodeViewModel viewModel,
        CancellationToken cancellationToken)
    {
        return this.ConfirmEmailChangeAsync(this.User.GetUserId(), viewModel, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("{userId}/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostAnonymousVerificationCode(
        [FromRoute] Guid userId,
        VerificationCodeViewModel viewModel,
        CancellationToken cancellationToken)
    {
        return await this.ConfirmEmailChangeAsync(userId, viewModel, cancellationToken);
    }

    private async Task<IActionResult> ConfirmEmailChangeAsync(
        Guid userId,
        VerificationCodeViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var validationResult = await viewModel.ValidateAsync<VerificationCodeViewModelValidator, VerificationCodeViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return await this.RenderVerificationCodeViewAsync(userId, cancellationToken);
        }

        var request = new ConfirmChangeEmailAddressRequest {
            VerificationCode = viewModel.VerificationCodeInput!,
        };

        try {
            var response = await usersApiClient.ConfirmChangeEmailAddress(userId, request, cancellationToken);
            return await this.HandleConfirmationResponseAsync(response, userId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Failed to confirm change email address for user {UserId}.", userId);
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }
    }

    private async Task<IActionResult> HandleConfirmationResponseAsync(
        Refit.IApiResponse<ConfirmChangeEmailAddressResponse> response,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) {
            if (response.Content?.HasWarning(ChangeEmailWarnings.EntraMfaSyncFailed) == true) {
                logger.LogError("Partially failed to change email address for user {UserId}: Entra MFA sync failed.", userId);
                return this.ErrorView(ProfileErrorPresets.EmailMfaSyncFailed(this.Url));
            }

            return this.RedirectToAction(nameof(Complete));
        }

        if (response.StatusCode == HttpStatusCode.BadRequest) {
            if (await response.IsProblemType(ChangeEmailErrors.NoPendingRequest.Code)) {
                // Keep API details private on this anonymous verification route.
                this.ModelState.AddModelError(
                    nameof(VerificationCodeViewModel.VerificationCodeInput),
                    "The verification code you entered is incorrect. Please check and try again.");

                this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = true;

                return await this.RenderVerificationCodeViewAsync(userId, cancellationToken);
            }

            await response.TryAddProblemDetailsToModelStateAsync(
                this.ModelState,
                VerificationCodeViewModel.RequestPropertyMap,
                fallbackField: nameof(VerificationCodeViewModel.VerificationCodeInput));

            return await this.RenderVerificationCodeViewAsync(userId, cancellationToken);
        }

        logger.LogError("Failed to change email address for user {UserId}. StatusCode: {StatusCode}", userId, response.StatusCode);
        return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
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
    public async Task<IActionResult> PostCancel(CancellationToken cancellationToken)
    {
        Refit.IApiResponse response;
        try {
            response = await usersApiClient.CancelChangeEmailAddress(this.User.GetUserId(), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Failed to cancel change email for user {UserId}.", this.User.GetUserId());
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }
        if (!response.IsSuccessStatusCode) {
            logger.LogError(
                "Failed to cancel change email for user {UserId}. StatusCode: {StatusCode}",
                this.User.GetUserId(),
                response.StatusCode);
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }

        this.SetFlashNotification(
            heading: "Email change cancelled",
            message: "As you did not complete the email change process, your email change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<IActionResult> RenderVerificationCodeViewAsync(Guid userId, CancellationToken cancellationToken)
    {
        Refit.IApiResponse<GetPendingChangeEmailResponse> response;
        try {
            response = await usersApiClient.GetPendingChangeEmail(userId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Failed to retrieve pending change email for user {UserId}.", userId);
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }

        if (response.StatusCode == HttpStatusCode.NotFound) {
            var emailAddress = this.TempData
                .Peek(VerificationCodeViewModel.NewEmailAddressTempDataKey)?.ToString() ?? string.Empty;
            return this.View("VerificationCode", new VerificationCodeViewModel {
                UserId = userId,
                NewEmailAddress = emailAddress,
            });
        }

        if (!response.IsSuccessStatusCode || response.Content is null) {
            logger.LogError("Failed to retrieve pending change email for user {UserId}. StatusCode: {StatusCode}",
                userId, response.StatusCode);
            return this.ErrorView(ProfileErrorPresets.EmailUpdateFailed(this.Url));
        }

        this.ModelState.SetModelValue(nameof(VerificationCodeViewModel.VerificationCodeInput), null, "");

        return this.View("VerificationCode", new VerificationCodeViewModel {
            UserId = userId,
            NewEmailAddress = response.Content.NewEmailAddress,
        });
    }
}
