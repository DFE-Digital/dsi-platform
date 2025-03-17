using System.Text;
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
    IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse> invalidateSelectOrganisationSessionRequest,
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
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        // If there are no options; invoke the callback right away.
        if (session.OrganisationOptions.Count() == 0) {
            return await this.SendErrorCallback(session, SelectOrganisationErrorCode.NoOptions);
        }

        // If there is only one option; invoke the callback right away.
        if (session.OrganisationOptions.Count() == 1) {
            // Invalidate the session since it is being handled now.
            await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
                SessionKey = sessionKey,
            });

            var selectedOrganisation = (await getOrganisationById.InvokeAsync(new() {
                OrganisationId = session.OrganisationOptions.First().Id
            })).Organisation;

            if (selectedOrganisation is null) {
                // The organisation does not exist; maybe it was deleted.
                return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
            }

            var selectionPayload = this.RemapSelectedOrganisationToCallbackData(selectedOrganisation, session.DetailLevel);
            return await this.SendCallback(session, selectionPayload);
        }

        // Present prompt to the user.
        return this.View(new SelectOrganisationViewModel {
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
        });
    }

    [HttpPost]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> PostIndex(string clientId, string sessionKey, SelectOrganisationViewModel viewModel)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
            SessionKey = sessionKey,
        });

        bool didUserSelectOptionThatWasPresented = session.OrganisationOptions.Any(
            option => option.Id == viewModel.SelectedOrganisationId);
        if (!didUserSelectOptionThatWasPresented) {
            return await this.HandleInvalidSessionAsync(clientId);
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
        string json = JsonSerializer.Serialize(payload, jsonSerializerOptions);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(json);

        var signingResponse = await createDigitalSignatureForPayload.InvokeAsync(new() {
            Payload = json,
        });

        return this.View("Callback", new CallbackViewModel {
            CallbackUrl = session.CallbackUrl,
            PayloadType = ((SelectOrganisationCallback)payload).Type,
            PayloadData = Convert.ToBase64String(payloadBytes),
            DigitalSignature = signingResponse.Signature.Signature,
            PublicKeyId = signingResponse.Signature.KeyId,
        });
    }

    private sealed record GetSessionResult()
    {
        public SelectOrganisationSessionData? Session { get; init; } = null;
        public IActionResult? RedirectActionResult { get; init; } = null;
    }

    private async Task<GetSessionResult> GetSessionAsync(string clientId, string sessionKey)
    {
        var session = (await getSelectOrganisationSessionByKey.InvokeAsync(new() {
            SessionKey = sessionKey,
        })).SessionData;

        if (session is null) {
            // Redirect when session does not exist.
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(clientId),
            };
        }

        if (clientId != session.ClientId) {
            // User has tampered with the clientId parameter of the URL.
            await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
                SessionKey = sessionKey,
            });
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(session.ClientId),
            };
        }

        return new GetSessionResult { Session = session };
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
            if (response.Application is not null) {
                returnUrl = response.Application.ServiceHomeUrl;
            }
        }

        return returnUrl;

    }
}
