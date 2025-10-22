using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their job title.
/// </summary>
public sealed class ChangeJobTitleController(
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet("/change-job-title")]
    public async Task<IActionResult> Index()
    {
        Guid userId = this.User.GetUserId();

        var profile = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        return this.View(new ChangeJobTitleViewModel {
            JobTitleInput = profile.JobTitle,
        });
    }

    [Authorize]
    public async Task<IActionResult> PostIndex()
    {
        this.SetFlashSuccess(
            heading: "Job title updated successfully",
            message: "The job title associated with your account has been updated."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> PostCancel()
    {
        this.SetFlashNotification(
            heading: "Job title change cancelled",
            message: "As you did not complete the job title change process, your job title change has been cancelled."
        );

        return this.RedirectToAction("Index", "Home");
    }
}
