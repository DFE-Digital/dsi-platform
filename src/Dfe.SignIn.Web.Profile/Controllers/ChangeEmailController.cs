using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their email address.
/// </summary>
[Route("/change-email")]
public sealed class ChangeEmailController(
    IOptionsMonitor<ApplicationOidcOptions> oidcOptionsAccessor,
    IOptionsMonitor<DistributedCacheInteractionLimiterOptions> limiterOptions,
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (this.HttpContext.Session.GetInt32(SessionKeys.IsInternalUser) == 1) {
            return this.NotFound();
        }

        return this.View("Index");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostIndex(ChangeEmailViewModel postModel, [FromQuery] bool? resend = false)
    {
        if (this.HttpContext.Session.GetInt32(SessionKeys.IsInternalUser) == 1) {
            return this.NotFound();
        }

        bool hideResendVerificationBanner = false;

        try {
            await this.MapInteractionRequest<InitiateChangeEmailAddressRequest>(postModel)
                .Use(request => request with {
                    ClientId = oidcOptionsAccessor.CurrentValue.ClientId,
                    UserId = this.User.GetUserId(),
                    IsSelfInvoked = true,
                })
                .InvokeAsync(interaction.DispatchAsync);

            if (resend == true) {
                this.SetFlashSuccess(
                    heading: "Verification code resent",
                    message: $"""
                We have sent an account verification email to {postModel.EmailAddressInput}.
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

    [Authorize]
    [HttpGet("verify")]
    public Task<IActionResult> VerificationCode()
    {
        return this.VerificationCodeAnonymous(this.User.GetUserId());
    }

    [AllowAnonymous]
    [HttpGet("{userId}/verify")]
    public async Task<IActionResult> VerificationCodeAnonymous(Guid userId)
    {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        return await this.VerificationCodeHelper(userId);
    }

    private async Task<IActionResult> VerificationCodeHelper(Guid userId)
    {
        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest { UserId = userId }
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
    public async Task<IActionResult> PostVerificationCode(Guid userId, VerificationCodeViewModel postModel)
    {
        try {
            await this.MapInteractionRequest<ConfirmChangeEmailAddressRequest>(postModel)
                .Use(request => request with { UserId = userId })
                .InvokeAsync(interaction.DispatchAsync);

            if (!this.ModelState.IsValid) {
                return await this.VerificationCodeHelper(userId);
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

    [Authorize]
    [HttpGet("complete")]
    public IActionResult Complete()
    {
        return this.View("Complete");
    }

    [Authorize]
    public IActionResult PostComplete()
    {
        this.SetFlashNotification(
            heading: "Email address updated successfully",
            message: "Please allow up to 10 minutes for the changes to take effect before signing back into your account."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [Authorize]
    [HttpPost("cancel")]
    public async Task<IActionResult> PostCancel()
    {
        await interaction.DispatchAsync(new CancelPendingChangeEmailAddressRequest {
            UserId = this.User.GetUserId(),
        });

        this.SetFlashNotification(
            heading: "Email change cancelled",
            message: "As you did not complete the email change process, your email change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
