using Dfe.SignIn.SelectOrganisation.Data;
using Dfe.SignIn.SelectOrganisation.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.SelectOrganisation.Web.Controllers;

/// <summary>
/// The controller for selecting an organisation.
/// </summary>
public sealed class SelectOrganisationController : Controller
{
    private readonly ISelectOrganisationSessionRetriever selectOrganisationRetriever;

    public SelectOrganisationController(
        ISelectOrganisationSessionRetriever selectOrganisationRetriever)
    {
        this.selectOrganisationRetriever = selectOrganisationRetriever;
    }

    public async Task<IActionResult> Index(string clientId, string sessionKey)
    {
        var session = await this.selectOrganisationRetriever.RetrieveSessionAsync(sessionKey);

        if (session == null) {
            // TODO: Redirect to home page of service identified by clientId.
            return this.View("InvalidSessionError");
        }

        if (clientId != session.ClientId) {
            throw new InvalidOperationException("Invalid client");
        }

        return this.View(new SelectOrganisationViewModel {
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
        });
    }
}
