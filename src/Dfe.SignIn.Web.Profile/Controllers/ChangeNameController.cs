using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Validation;
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
    public async Task<IActionResult> PostIndex(ChangeNameViewModel viewModel)
    {
        var validationResult = await viewModel.ValidateAsync<ChangeNameViewModelValidator, ChangeNameViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.Index();
        }

        var userDetails = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();
        if (string.Equals(viewModel.FirstNameInput, userDetails.FirstName, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(viewModel.LastNameInput, userDetails.LastName, StringComparison.InvariantCultureIgnoreCase)) {
            return this.Index();
        }

        if (!await this.TryChangeNameAsync(viewModel)) {
            return this.Index();
        }

        this.SetFlashSuccess(
            heading: "Name updated successfully",
            message: "The name associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private async Task<bool> TryChangeNameAsync(ChangeNameViewModel viewModel)
    {
        try {
            var request = new ChangeNameRequest {
                FirstName = viewModel.FirstNameInput ?? string.Empty,
                LastName = viewModel.LastNameInput ?? string.Empty,
            };

            var response = await usersApiClient.ChangeName(this.User.GetUserId(), request);

            if (response.IsSuccessStatusCode) {
                return true;
            }

            logger.LogError("Failed to change name for user {UserId}. StatusCode: {StatusCode}", this.User.GetUserId(), response.StatusCode);
            this.ModelState.AddModelError(string.Empty, "We couldn't save your name right now. Please try again.");
            return false;
        }
        catch (Exception ex) {
            logger.LogError(ex, "An error occurred while changing the user's name.");
            this.ModelState.AddModelError(string.Empty, "We couldn't save your name right now. Please try again.");
            return false;
        }
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
