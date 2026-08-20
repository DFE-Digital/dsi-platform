using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
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
    IUsersApiClient usersApiClient,
    ILogger<HomeController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        GetPendingChangeEmailResponse? pendingResponse = null;
        try {
            pendingResponse = await usersApiClient.GetPendingChangeEmail(userProfileFeature.UserId);
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            // No pending email change — this is expected
        }
        catch (Exception ex) {
            logger.LogError(ex, "Failed to retrieve pending change email for user {UserId}", userProfileFeature.UserId);
        }

        return this.View(new HomeViewModel {
            FullName = $"{userProfileFeature.FirstName} {userProfileFeature.LastName}",
            JobTitle = userProfileFeature.JobTitle,
            EmailAddress = userProfileFeature.EmailAddress,
            PendingEmailAddress = pendingResponse?.NewEmailAddress,
        });
    }
}
