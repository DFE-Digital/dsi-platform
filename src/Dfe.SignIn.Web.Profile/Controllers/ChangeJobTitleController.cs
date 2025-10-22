using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their job title.
/// </summary>
[Route("/change-job-title")]
public sealed class ChangeJobTitleController(
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        Guid userId = this.User.GetUserId();

        var profile = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        return this.View("Index", new ChangeJobTitleViewModel {
            JobTitleInput = profile.JobTitle,
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostIndex(ChangeJobTitleViewModel viewModel)
    {
        await this.MapInteractionRequest<ChangeJobTitleRequest>(viewModel)
            .Use(request => request with {
                UserId = this.User.GetUserId(),
            })
            .InvokeAsync(interaction.DispatchAsync);

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Job title updated successfully",
            message: "The job title associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [Authorize]
    [HttpPost("cancel")]
    public IActionResult PostCancel()
    {
        this.SetFlashNotification(
            heading: "Job title change cancelled",
            message: "As you did not complete the job title change process, your job title change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
