using System.Text.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.Core.Models.Organisations.Interactions;
using Dfe.SignIn.SelectOrganisation.Data;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Models;
using Dfe.SignIn.SelectOrganisation.Web.Signing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Controllers;

/// <summary>
/// The controller for selecting an organisation.
/// </summary>
public sealed class SelectOrganisationController(
    IOptions<ApplicationOptions> applicationOptionsAccessor,
    IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse> getApplicationByClientId,
    IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse> getOrganisationById,
    ISelectOrganisationSessionRetriever selectOrganisationRetriever,
    ICallbackPayloadSigner callbackPayloadSigner
) : Controller
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [HttpGet]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(string clientId, string sessionKey)
    {
        var session = await selectOrganisationRetriever.RetrieveSessionAsync(sessionKey);

        if (session == null) {
            return await this.HandleInvalidSessionAsync(clientId);
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
        var session = await selectOrganisationRetriever.RetrieveSessionAsync(sessionKey);

        if (session == null) {
            return await this.HandleInvalidSessionAsync(clientId);
        }
        if (clientId != session.ClientId) {
            throw new InvalidOperationException("Invalid client");
        }

        var organisation = await getOrganisationById.InvokeAsync(new() {
            Id = viewModel.SelectedOrganisationId,
        });

        var json = JsonSerializer.Serialize(organisation, jsonSerializerOptions);
        var signature = callbackPayloadSigner.CreateDigitalSignature(json);

        return this.View("Callback", new SelectOrganisationCallbackViewModel {
            CallbackUrl = session.CallbackUrl,
            PayloadData = json,
            DigitalSignature = signature.Signature,
            PublicKeyId = signature.KeyId,
        });
    }

    private async Task<IActionResult> HandleInvalidSessionAsync(string? clientId)
    {
        return this.View("InvalidSessionError", new InvalidSessionViewModel {
            ReturnUrl = await this.GetServiceHomeUrlAsync(clientId),
        });
    }

    private async Task<Uri> GetServiceHomeUrlAsync(string? clientId)
    {
        var returnUrl = applicationOptionsAccessor.Value.ServicesUrl;

        if (!string.IsNullOrEmpty(clientId)) {
            var response = await getApplicationByClientId.InvokeAsync(new GetApplicationByClientIdRequest {
                ClientId = clientId,
            });
            if (response.Application != null) {
                returnUrl = response.Application.ServiceHomeUrl;
            }
        }

        return returnUrl;

    }
}
