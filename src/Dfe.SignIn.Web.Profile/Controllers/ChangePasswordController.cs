using Dfe.SignIn.Base.Framework.OperationResults;
using Dfe.SignIn.Core.Contracts.Audit;
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
    IAuditWriter auditWriter,
    ILogger<ChangePasswordController> logger
) : Controller
{
    private enum PasswordChangeOutcome
    {
        Succeeded,
        Invalid,
        Failed,
        AuthenticationRequired,
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool reauthenticate = false)
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await associatedAccountAuthService.AuthenticateAssociatedAccount(this,
                [AssociatedAccountConstants.DefaultGraphScope],
                SelectAssociatedReturnLocation.ChangePassword, force: reauthenticate);

            if (actionResult is not null) {
                return actionResult;
            }
        }

        return this.PasswordForm();
    }

    private IActionResult PasswordForm()
    {
        // Never redisplay submitted passwords, but retain their validation errors.
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.CurrentPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.NewPasswordInput), null, "");
        this.ModelState.SetModelValue(nameof(ChangePasswordViewModel.ConfirmNewPasswordInput), null, "");

        return this.View("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(ChangePasswordViewModel viewModel, CancellationToken cancellationToken)
    {
        var validationResult = await viewModel.ValidateAsync<ChangePasswordViewModelValidator, ChangePasswordViewModel>();

        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);

            await this.WriteAuditAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                Message = "Change password attempt failed",
                EventName = AuditChangePasswordEventNames.PasswordValidation,
                WasFailure = true
            });

            return this.PasswordForm();
        }

        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        var outcome = userProfileFeature.IsEntra
            ? await this.TryChangeEntraPasswordAsync(viewModel, cancellationToken)
            : await this.TryChangeLocalPasswordAsync(viewModel, userProfileFeature.UserId, cancellationToken);

        if (outcome == PasswordChangeOutcome.Invalid) {
            return this.PasswordForm();
        }

        if (outcome == PasswordChangeOutcome.AuthenticationRequired) {
            return this.ErrorView(ProfileErrorPresets.PasswordAuthenticationRequired(this.Url));
        }

        if (outcome == PasswordChangeOutcome.Failed) {
            return this.ErrorView(ProfileErrorPresets.PasswordUpdateFailed(this.Url));
        }

        this.SetFlashSuccess(
            heading: "Password changed successfully",
            message: "The password associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<PasswordChangeOutcome> TryChangeEntraPasswordAsync(ChangePasswordViewModel viewModel, CancellationToken cancellationToken)
    {
        var result = await this.ExecuteEntraPasswordChangeAsync(viewModel, cancellationToken);
        return await this.HandleEntraPasswordChangeResultAsync(result);
    }

    private async Task<OperationResult> ExecuteEntraPasswordChangeAsync(ChangePasswordViewModel viewModel, CancellationToken cancellationToken)
    {
        try {
            var graphAccessToken = await associatedAccountAuthService.CreateAccessTokenForAssociatedAccount(this,
                [AssociatedAccountConstants.DefaultGraphScope],
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Provided graph token for user is null");

            return await entraChangePasswordService.ChangePasswordAsync(
                viewModel.CurrentPasswordInput!,
                viewModel.NewPasswordInput!,
                graphAccessToken,
                cancellationToken);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status403Forbidden) {
            logger.LogWarning(ex, "Associated account authentication failed during password change.");
            return OperationResult.Failure(EntraPasswordErrors.AuthenticationRequired);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "An error occurred while changing the user's password.");
            return OperationResult.Failure(EntraPasswordErrors.Unexpected(ex.Message));
        }
    }

    private async Task<PasswordChangeOutcome> HandleEntraPasswordChangeResultAsync(OperationResult result)
    {
        if (result.IsSuccess) {
            await this.WriteAuditAsync(new WriteToAuditRequest {
                Message = "Successfully changed password",
                EventCategory = AuditEventCategoryNames.ChangePassword
            });
            return PasswordChangeOutcome.Succeeded;
        }

        if (result.Error.Code == EntraPasswordErrors.AuthenticationRequiredCode) {
            return PasswordChangeOutcome.AuthenticationRequired;
        }

        var field = result.Error.Code switch {
            EntraPasswordErrors.InvalidCurrentPasswordCode => nameof(ChangePasswordViewModel.CurrentPasswordInput),
            EntraPasswordErrors.PasswordPolicyViolationCode => nameof(ChangePasswordViewModel.NewPasswordInput),
            _ => null,
        };

        if (field is null) {
            logger.LogWarning("Entra password change failed with code {ErrorCode}.", result.Error.Code);
            return PasswordChangeOutcome.Failed;
        }

        this.ModelState.AddModelError(field, result.Error.Description);
        if (result.Error.Code == EntraPasswordErrors.InvalidCurrentPasswordCode) {
            await this.WriteAuditAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangePassword,
                EventName = AuditChangePasswordEventNames.IncorrectPassword,
                WasFailure = true,
                Message = "Failed changed password. Incorrect current password"
            });
        }

        await this.AuditEntraValidationFailureAsync();
        return PasswordChangeOutcome.Invalid;
    }

    private Task AuditEntraValidationFailureAsync() => this.WriteAuditAsync(new WriteToAuditRequest {
        Message = "Change password attempt failed.",
        EventCategory = AuditEventCategoryNames.ChangePassword,
        EventName = AuditChangePasswordEventNames.PasswordValidation,
        WasFailure = true
    });

    private async Task WriteAuditAsync(WriteToAuditRequest request)
    {
        try {
            await auditWriter.Log(request);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            // Audit delivery failure must not misrepresent the password operation's outcome.
            logger.LogError(ex, "Failed to publish password change audit event {EventName}.", request.EventName);
        }
    }

    private async Task<PasswordChangeOutcome> TryChangeLocalPasswordAsync(ChangePasswordViewModel viewModel, Guid userId, CancellationToken cancellationToken)
    {
        var request = new ChangePasswordRequest {
            CurrentPassword = viewModel.CurrentPasswordInput!,
            NewPassword = viewModel.NewPasswordInput!,
            ConfirmNewPassword = viewModel.ConfirmNewPasswordInput!,
        };

        try {
            var response = await usersApiClient.ChangePassword(userId, request, cancellationToken);

            if (response.IsSuccessStatusCode) {
                return PasswordChangeOutcome.Succeeded;
            }

            logger.LogWarning("Failed to change password for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
                await response.TryAddProblemDetailsToModelStateAsync(this.ModelState, ChangePasswordViewModel.RequestPropertyMap);
                if (this.ModelState.Any(entry => ChangePasswordViewModel.RequestPropertyMap.Values.Contains(entry.Key)
                    && entry.Value is { Errors.Count: > 0 })) {
                    return PasswordChangeOutcome.Invalid;
                }
            }

            return PasswordChangeOutcome.Failed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "An error occurred while changing the user's password.");
            return PasswordChangeOutcome.Failed;
        }
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
