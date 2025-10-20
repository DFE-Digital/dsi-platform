using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Humanizer;
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
    IOptionsMonitor<DistributedCacheInteractionLimiterOptions> limiterOptions,
    IInteractionDispatcher interaction
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
        [FromQuery] bool? resend, ChangeEmailViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        bool hideResendVerificationBanner = false;

        try {
            await this.MapInteractionRequest<InitiateChangeEmailAddressRequest>(viewModel)
                .Use(request => request with {
                    ClientId = oidcOptionsAccessor.CurrentValue.ClientId,
                    UserId = this.User.GetUserId(),
                    IsSelfInvoked = true,
                })
                .InvokeAsync(interaction.DispatchAsync, cancellationToken);

            if (resend == true) {
                this.SetFlashSuccess(
                    heading: "Verification code resent",
                    message: $"""
                    We have sent an account verification email to {viewModel.EmailAddressInput}.
                    If the email address you provided is valid you will receive an email containing a verification code.
                    """
                );
            }

            if (!this.ModelState.IsValid) {
                return this.View("Index");
            }
        }
        catch (InteractionRejectedByLimiterException) {
            var options = limiterOptions.Get<InitiateChangeEmailAddressRequest>();
            var timePeriod = TimeSpan.FromSeconds(options.TimePeriodInSeconds).Humanize();
            this.SetFlashNotification(
                heading: "Verification code limit reached",
                message: $"""
                For security, only {options.InteractionsPerTimePeriod} verification codes requests can be sent.
                Wait {timePeriod} before raising another request, or enter your verification code below.
                """
            );
            hideResendVerificationBanner = true;
        }

        this.TempData[VerificationCodeViewModel.HideResendVerificationTempDataKey] = hideResendVerificationBanner;

        return this.RedirectToAction(nameof(VerificationCode));
    }

    [HttpGet("verify")]
    public Task<IActionResult> VerificationCode(
        CancellationToken cancellationToken = default)
    {
        return this.VerificationCodeAnonymous(this.User.GetUserId(), cancellationToken);
    }

    [AllowAnonymous]
    [HttpGet("{userId}/verify")]
    public async Task<IActionResult> VerificationCodeAnonymous(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        return await this.VerificationCodeHelper(userId, cancellationToken);
    }

    private async Task<IActionResult> VerificationCodeHelper(Guid userId, CancellationToken cancellationToken)
    {
        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest {
                UserId = userId
            }, cancellationToken
        ).To<GetPendingChangeEmailAddressResponse>();

        var pendingChange = pendingChangeEmailAddressResponse.PendingChangeEmailAddress;
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
        [FromRoute] Guid userId, VerificationCodeViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        try {
            await this.MapInteractionRequest<ConfirmChangeEmailAddressRequest>(viewModel)
                .Use(request => request with { UserId = userId })
                .InvokeAsync(interaction.DispatchAsync, cancellationToken);

            if (!this.ModelState.IsValid) {
                return await this.VerificationCodeHelper(userId, cancellationToken);
            }

            return this.RedirectToAction(nameof(Complete));
        }
        catch (NoPendingChangeEmailException) {
            return this.RedirectToAction(nameof(Index));
        }
        catch (FailedToUpdateAuthenticationMethodException) {
            return this.ErrorView("ErrorUpdateAuthenticationMethod");
        }
        catch {
            return this.ErrorView("ErrorUpdateEmailAddress");
        }
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
    public async Task<IActionResult> PostCancel(
        CancellationToken cancellationToken = default)
    {
        await interaction.DispatchAsync(
            new CancelPendingChangeEmailAddressRequest {
                UserId = this.User.GetUserId(),
            }, cancellationToken
        );

        this.SetFlashNotification(
            heading: "Email change cancelled",
            message: "As you did not complete the email change process, your email change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
