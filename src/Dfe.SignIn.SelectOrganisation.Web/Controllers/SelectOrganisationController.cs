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

    [HttpGet]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(string clientId, string sessionKey)
    {
        var session = await this.selectOrganisationRetriever.RetrieveSessionAsync(sessionKey);

        if (session == null) {
            // TODO: Redirect to home page of service identified by clientId.
            return this.View("InvalidSessionError", new InvalidSessionViewModel {
                ReturnUrl = new Uri("https://services.localhost"),
            });
        }

        if (clientId != session.ClientId) {
            throw new InvalidOperationException("Invalid client");
        }

        return this.View(new SelectOrganisationViewModel {
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
        });
    }

    [HttpPost]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> PostIndex(string clientId, string sessionKey, SelectOrganisationViewModel viewModel)
    {
        var session = await this.selectOrganisationRetriever.RetrieveSessionAsync(sessionKey);

        if (session == null) {
            // TODO: Redirect to home page of service identified by clientId.
            return this.View("InvalidSessionError", new InvalidSessionViewModel {
                ReturnUrl = new Uri("https://services.localhost"),
            });
        }

        if (clientId != session.ClientId) {
            throw new InvalidOperationException("Invalid client");
        }

        // TODO: Get organisation information from mid-tier API.
        // TODO: Serialize data payload into a JSON encoded string.
        // TODO: Create digital signature for the serialized data payload.

        return this.View("Callback", new SelectOrganisationCallbackViewModel {
            CallbackUrl = session.CallbackUrl,
            PayloadData = $"PAYLOAD: Selected organisation: {viewModel.SelectedOrganisationId}",
            DigitalSignature = "...",
            PublicKeyId = "key1",
        });
    }
}
