using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller for the main landing page of the profile.
/// </summary>
[Route("/")]
public sealed class HomeController(
    IInteractionDispatcher interaction
) : Controller
{
    [Authorize]
    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        Guid userId = this.User.GetUserId();

        var profileResponse = await interaction.DispatchAsync(
            new GetUserProfileRequest { UserId = userId }
        ).To<GetUserProfileResponse>();

        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest { UserId = userId }
        ).To<GetPendingChangeEmailAddressResponse>();

        return this.View(new HomeViewModel {
            FullName = $"{profileResponse.GivenName} {profileResponse.Surname}",
            JobTitle = profileResponse.JobTitle,
            EmailAddress = profileResponse.EmailAddress,
            PendingEmailAddress = pendingChangeEmailAddressResponse.PendingChangeEmailAddress?.NewEmailAddress,
        });
    }
}
