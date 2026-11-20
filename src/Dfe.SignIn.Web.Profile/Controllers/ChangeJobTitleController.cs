using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their job title.
/// </summary>
[Authorize]
[Route("/change-job-title")]
public sealed class ChangeJobTitleController(
    IUsersApiClient usersApiClient,
    ILogger<ChangeJobTitleController> logger
) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        return this.View("Index", new ChangeJobTitleViewModel {
            JobTitleInput = userProfileFeature.JobTitle,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        ChangeJobTitleViewModel viewModel, CancellationToken cancellationToken)
    {
        var validationResult = await viewModel.ValidateAsync<ChangeJobTitleViewModelValidator, ChangeJobTitleViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.Index();
        }

        var userId = this.User.GetUserId();
        try {
            var response = await usersApiClient.ChangeJobTitle(userId, new ChangeJobTitleRequest {
                NewJobTitle = viewModel.JobTitleInput
            }, cancellationToken);
            if (!response.IsSuccessStatusCode) {
                logger.LogError("Failed to change job title for user {UserId}. StatusCode: {StatusCode}", userId, response.StatusCode);
                return this.ErrorView(ProfileErrorPresets.JobTitleUpdateFailed(this.Url));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "An exception occurred while changing job title for user {UserId}", userId);
            return this.ErrorView(ProfileErrorPresets.JobTitleUpdateFailed(this.Url));
        }

        this.SetFlashSuccess(
            heading: "Job title updated successfully",
            message: "The job title associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public IActionResult PostCancel()
    {
        this.SetFlashNotification(
            heading: "Job title change cancelled",
            message: "As you did not complete the job title change process, your job title change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
