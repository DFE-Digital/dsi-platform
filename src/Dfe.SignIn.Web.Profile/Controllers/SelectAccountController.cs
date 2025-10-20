using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Controllers;

/// <summary>
/// The controller that prompts the user when they are not signed in with the
/// external account that is associated with their DfE Sign-In account.
/// </summary>
[Authorize]
[Route("/select-account")]
public sealed class SelectAccountController(
    ISelectAssociatedAccountHelper selectAssociatedAccountHelper
) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(SelectAssociatedAccountViewModel viewModel)
    {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        var userProfileFeature = this.HttpContext.Features.GetRequiredFeature<IUserProfileFeature>();

        if (userProfileFeature.IsEntra) {
            var actionResult = await selectAssociatedAccountHelper.AuthenticateAssociatedAccount(
                this, [], viewModel.RedirectUri, force: true);
            if (actionResult is not null) {
                return actionResult;
            }
        }

        return this.Redirect(viewModel.RedirectUri ?? "/");
    }
}
