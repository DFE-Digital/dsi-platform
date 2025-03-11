using System.Text.Json;
using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.Organisations.Interactions;
using Dfe.SignIn.Core.Models.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.SelectOrganisation.Web.Configuration;
using Dfe.SignIn.SelectOrganisation.Web.Models;
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
    IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse> getSelectOrganisationSessionByKey,
    IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse> createDigitalSignatureForPayload,
    IMapper mapper
) : Controller
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [HttpGet]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(string clientId, string sessionKey)
    {
        var session = await this.GetSessionData(clientId, sessionKey);
        if (session == null) {
            return await this.HandleInvalidSessionAsync(clientId);
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
        var session = await this.GetSessionData(clientId, sessionKey);
        if (session == null) {
            return await this.HandleInvalidSessionAsync(clientId);
        }

        // TODO: Invalidate session!

        bool didUserSelectOptionThatWasPresented = session.OrganisationOptions.Any(
            option => option.Id == viewModel.SelectedOrganisationId);
        if (!didUserSelectOptionThatWasPresented) {
            return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
        }

        var selectedOrganisation = (await getOrganisationById.InvokeAsync(new() {
            OrganisationId = viewModel.SelectedOrganisationId,
        })).Organisation;
        if (selectedOrganisation is null) {
            return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
        }

        var selectionPayload = this.RemapSelectedOrganisationToCallbackData(selectedOrganisation, session.DetailLevel);
        return await this.SendCallback(session, selectionPayload);
    }

    private Task<IActionResult> SendErrorCallback(
        SelectOrganisationSessionData session, SelectOrganisationErrorCode errorCode)
    {
        return this.SendCallback(session, new SelectOrganisationCallbackError {
            Type = PayloadTypeConstants.Error,
            Code = errorCode,
        });
    }

    private async Task<IActionResult> SendCallback(SelectOrganisationSessionData session, object payload)
    {
        var json = JsonSerializer.Serialize(payload, jsonSerializerOptions);
        var signingResponse = await createDigitalSignatureForPayload.InvokeAsync(new() {
            Payload = json,
        });

        return this.View("Callback", new CallbackViewModel {
            CallbackUrl = session.CallbackUrl,
            PayloadType = ((SelectOrganisationCallback)payload).Type,
            PayloadData = json,
            DigitalSignature = signingResponse.Signature.Signature,
            PublicKeyId = signingResponse.Signature.KeyId,
        });
    }

    private async Task<SelectOrganisationSessionData?> GetSessionData(string clientId, string sessionKey)
    {
        var sessionResponse = await getSelectOrganisationSessionByKey.InvokeAsync(new() {
            SessionKey = sessionKey,
        });

        if (sessionResponse.SessionData == null) {
            return null;
        }
        if (clientId != sessionResponse.SessionData.ClientId) {
            throw new InvalidOperationException("Invalid client");
        }

        return sessionResponse.SessionData;
    }

    private object RemapSelectedOrganisationToCallbackData(
        OrganisationModel selectedOrganisation, OrganisationDetailLevel detailLevel)
    {
        return detailLevel switch {
            OrganisationDetailLevel.Basic => mapper.Map<SelectOrganisationCallbackBasic>(selectedOrganisation)
                with { Type = PayloadTypeConstants.Basic },
            OrganisationDetailLevel.Id => mapper.Map<SelectOrganisationCallbackId>(selectedOrganisation)
                with { Type = PayloadTypeConstants.Id },
            OrganisationDetailLevel.Extended => mapper.Map<SelectOrganisationCallbackExtended>(selectedOrganisation)
                with { Type = PayloadTypeConstants.Extended },
            OrganisationDetailLevel.Legacy => mapper.Map<SelectOrganisationCallbackLegacy>(selectedOrganisation)
                with { Type = PayloadTypeConstants.Legacy },
            _ => mapper.Map<SelectOrganisationCallbackBasic>(selectedOrganisation),
        };
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
