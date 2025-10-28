using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that allows the user to change their name.
/// </summary>
public sealed class ChangeNameController(
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet("/change-name")]
    public async Task<IActionResult> Index()
    {
        Guid userId = this.User.GetUserId();

        var profile = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        return this.View("Index", new ChangeNameViewModel {
            FirstNameInput = profile.GivenName,
            LastNameInput = profile.Surname,
        });
    }

    [Authorize]
    public async Task<IActionResult> PostIndex(ChangeNameViewModel viewModel)
    {
        try {
            await interaction.DispatchAsync(
                new ChangeNameRequest {
                    UserId = this.User.GetUserId(),
                    GivenName = viewModel.FirstNameInput!,
                    Surname = viewModel.LastNameInput!,
                }
            );
        }
        catch (InvalidRequestException ex) {
            this.ModelState.AddFrom(ex.ValidationResults, new() {
                [nameof(ChangeNameRequest.GivenName)] = nameof(ChangeNameViewModel.FirstNameInput),
                [nameof(ChangeNameRequest.Surname)] = nameof(ChangeNameViewModel.LastNameInput),
            });
            this.ModelState.ThrowIfNoErrorsRecorded(ex);
        }

        if (!this.ModelState.IsValid) {
            return await this.Index();
        }

        this.SetFlashSuccess(
            heading: "Name updated successfully",
            message: "The name associated with your account has been updated."
        );

        return this.RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> PostCancel()
    {
        this.SetFlashNotification(
            heading: "Name change cancelled",
            message: "As you did not complete the name change process, your name change has been cancelled."
        );

        return this.RedirectToAction("Index", "Home");
    }
}
