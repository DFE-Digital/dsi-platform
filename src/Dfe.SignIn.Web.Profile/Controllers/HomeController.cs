using System.Net;
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        GetPendingChangeEmailResponse? pendingResponse = null;
        try {
            var response = await usersApiClient.GetPendingChangeEmail(userProfileFeature.UserId, cancellationToken);
            if (response.IsSuccessStatusCode && response.Content is not null) {
                pendingResponse = response.Content;
            }
            else if (response.StatusCode != HttpStatusCode.NotFound) {
                logger.LogError("Failed to retrieve pending change email for user {UserId}. StatusCode: {StatusCode}",
                    userProfileFeature.UserId, response.StatusCode);
                return this.ErrorView(ProfileErrorPresets.ProfileLoadFailed(this.Url));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Failed to retrieve pending change email for user {UserId}", userProfileFeature.UserId);
            return this.ErrorView(ProfileErrorPresets.ProfileLoadFailed(this.Url));
        }

        return this.View(new HomeViewModel {
            FullName = $"{userProfileFeature.FirstName} {userProfileFeature.LastName}",
            JobTitle = userProfileFeature.JobTitle,
            EmailAddress = userProfileFeature.EmailAddress,
            PendingEmailAddress = pendingResponse?.NewEmailAddress,
        });
    }
}
