using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.AspNetCore.Authorization;
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

        var actionResult = await selectAssociatedAccountHelper.AuthenticateAssociatedAccount(
            this, [], viewModel.RedirectUri, force: true);
        if (actionResult is not null) {
            return actionResult;
        }

        return this.Redirect(viewModel.RedirectUri ?? "/");
    }
}
