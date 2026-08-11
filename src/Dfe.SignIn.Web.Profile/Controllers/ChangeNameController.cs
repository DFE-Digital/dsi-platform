using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using FluentValidation;
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
    ISelectAssociatedAccountHelper selectAssociatedAccountHelper,
    IGraphApiChangeUserPersonalDetails graphApiChangeUserPersonalDetails,
    ILogger<ChangeNameController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await selectAssociatedAccountHelper.AuthenticateAssociatedAccount(
                this, ["https://graph.microsoft.com/.default"], SelectAssociatedReturnLocation.ChangeNameDetails);
            if (actionResult is not null) {
                return actionResult;
            }
        }

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

        var userDetails = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (!validationResult.IsValid) {
            validationResult.AddToModelState(this.ModelState);
            return await this.Index();
        }

        if (string.IsNullOrEmpty(viewModel.FirstNameInput) || string.IsNullOrEmpty(viewModel.LastNameInput)) {
            return await this.Index();
        }

        if (viewModel.FirstNameInput?.ToLower() == userDetails.FirstName.ToLower() &&
        viewModel.LastNameInput?.ToLower() == userDetails.LastName.ToLower()) {
            return await this.Index();
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
            return await this.Index();
        }

        if (userDetails.IsEntra) {

            try {
                GraphAccessToken? graphAccessToken = null;
                graphAccessToken = await selectAssociatedAccountHelper.CreateAccessTokenForAssociatedAccount(
                    this, ["https://graph.microsoft.com/.default"]) ?? throw new Exception("Provided graph token for user is null");

                await graphApiChangeUserPersonalDetails.ChangeName(viewModel.FirstNameInput!,
                    viewModel.LastNameInput!, graphAccessToken);
            }

            catch (Exception ex) {
                await this.Rollback(userDetails.FirstName, userDetails.LastName);
                logger.LogError(ex, "An error occurred while changing the user's name.");
                this.ModelState.AddModelError(string.Empty, "We couldn't save your name right now. Please try again.");
                return await this.Index();
            }
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

    /// <summary>
    /// If entra fails, we apply a roll back to prevent de-sync issues.
    /// </summary>
    /// <param name="forename"></param>
    /// <param name="surname"></param>
    /// <returns></returns>
    private async Task Rollback(string forename, string surname)
    {
        var request = new ChangeNameRequest {
            UserId = this.User.GetUserId(),
            FirstName = forename,
            LastName = surname
        };

        await usersApiClient.ChangeName(request);
    }
}
