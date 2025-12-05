using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their first and last name.
/// </summary>
[Authorize]
[Route("/change-name")]
public sealed class ChangeNameController(
    IInteractionDispatcher interaction
) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        return this.View("Index", new ChangeNameViewModel {
            FirstNameInput = userProfileFeature.GivenName,
            LastNameInput = userProfileFeature.Surname,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        ChangeNameViewModel viewModel)
    {
        await interaction.MapRequestFromViewModel<ChangeNameRequest>(this, viewModel)
            .Use(request => request with {
                UserId = this.User.GetUserId(),
            })
            .DispatchAsync();

        if (!this.ModelState.IsValid) {
            return this.Index();
        }

        this.SetFlashSuccess(
            heading: "Name updated successfully",
            message: "The name associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public IActionResult PostCancel()
    {
        this.SetFlashNotification(
            heading: "Name change cancelled",
            message: "As you did not complete the name change process, your name change has been cancelled."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }
}
