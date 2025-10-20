using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
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
    IInteractionDispatcher interaction
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
        ChangeJobTitleViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        await this.MapInteractionRequest<ChangeJobTitleRequest>(viewModel)
            .Use(request => request with {
                UserId = this.User.GetUserId(),
            })
            .InvokeAsync(interaction.DispatchAsync, cancellationToken);

        if (!this.ModelState.IsValid) {
            return this.Index();
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
