using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller for the main landing page of the profile.
/// </summary>
[Authorize]
[Route("/")]
public sealed class HomeController(
    IInteractionDispatcher interaction
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        var pendingChangeEmailAddressResponse = await interaction.DispatchAsync(
            new GetPendingChangeEmailAddressRequest {
                UserId = userProfileFeature.UserId,
            }, cancellationToken
        ).To<GetPendingChangeEmailAddressResponse>();

        return this.View(new HomeViewModel {
            FullName = $"{userProfileFeature.GivenName} {userProfileFeature.Surname}",
            JobTitle = userProfileFeature.JobTitle,
            EmailAddress = userProfileFeature.EmailAddress,
            PendingEmailAddress = pendingChangeEmailAddressResponse.PendingChangeEmailAddress?.NewEmailAddress,
        });
    }
}
