using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
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
    IUsersApiClient usersApiClient
) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        return this.View("Index", new ChangeNameViewModel {
            FirstNameInput = userProfileFeature.FirstName,
            LastNameInput = userProfileFeature.LastName,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        ChangeNameViewModel viewModel)
    {
        var request = new ChangeNameRequest {
            UserId = this.User.GetUserId(),
            FirstName = viewModel.FirstNameInput ?? string.Empty,
            LastName = viewModel.LastNameInput ?? string.Empty,
        };

        try {
            await usersApiClient.ChangeName(request);
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest) {
            await this.ModelState.TryAddValidationErrorsAsync(ex);
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
