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
    public async Task<IActionResult> PostIndex(ChangeNameViewModel viewModel, CancellationToken cancellationToken)
    {
        var validationResult = await viewModel.ValidateAsync<ChangeNameViewModelValidator, ChangeNameViewModel>();
        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return this.Index();
        }

        if (!this.HasNameChanged(viewModel)) {
            return this.Index();
        }

        var request = new ChangeNameRequest {
            FirstName = viewModel.FirstNameInput ?? string.Empty,
            LastName = viewModel.LastNameInput ?? string.Empty,
        };

        var response = await usersApiClient.ChangeName(this.User.GetUserId(), request, cancellationToken);

        if (!response.IsSuccessStatusCode) {
            logger.LogError("Failed to change name for user {UserId}. StatusCode: {StatusCode}", this.User.GetUserId(), response.StatusCode);
            this.ModelState.AddModelError(string.Empty, "We couldn't save your name right now. Please try again.");
            return this.Index();
        }

        this.SetFlashSuccess(
            heading: "Name updated successfully",
            message: "The name associated with your account has been updated."
        );

        return this.RedirectToAction(nameof(HomeController.Index), MvcNaming.Controller<HomeController>());
    }

    private bool HasNameChanged(ChangeNameViewModel viewModel)
    {
        var userDetails = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        var firstNameChanged = !string.Equals(viewModel.FirstNameInput, userDetails.FirstName, StringComparison.OrdinalIgnoreCase);
        var lastNameChanged = !string.Equals(viewModel.LastNameInput, userDetails.LastName, StringComparison.OrdinalIgnoreCase);

        return firstNameChanged || lastNameChanged;
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
