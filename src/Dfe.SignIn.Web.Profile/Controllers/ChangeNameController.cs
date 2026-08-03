using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using FluentValidation.AspNetCore;
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
    IUsersApiClient usersApiClient,
    IValidator<ChangeNameViewModel> changeNameValidator,
    ILogger<ChangeNameController> logger
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
        var validationResult = await changeNameValidator.ValidateAsync(viewModel);

        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.Index();
        }

        try {
            var request = new ChangeNameRequest {
                UserId = this.User.GetUserId(),
                FirstName = viewModel.FirstNameInput ?? string.Empty,
                LastName = viewModel.LastNameInput ?? string.Empty,
            };

            await usersApiClient.ChangeName(request);
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred while changing the user's name.");
            this.ModelState.AddModelError(string.Empty, "We couldn't save your name right now. Please try again.");
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
