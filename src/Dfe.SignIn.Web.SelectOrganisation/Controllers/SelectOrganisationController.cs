using System.Text;
using System.Text.Json;
using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Applications.Interactions;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations.Interactions;
using Dfe.SignIn.Core.InternalModels.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Web.SelectOrganisation.Configuration;
using Dfe.SignIn.Web.SelectOrganisation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.SelectOrganisation.Controllers;

/// <summary>
/// The controller for selecting an organisation.
/// </summary>
public sealed class SelectOrganisationController(
    IOptions<ApplicationOptions> applicationOptionsAccessor,
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor,
    IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse> getApplicationByClientId,
    IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse> getOrganisationById,
    IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse> getSelectOrganisationSessionByKey,
    IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse> invalidateSelectOrganisationSessionRequest,
    IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse> createDigitalSignatureForPayload,
    IMapper mapper
) : Controller
{
    [HttpGet]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(
        string clientId,
        string sessionKey,
        CancellationToken cancellationToken = default)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey, cancellationToken);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        // If there are no options; invoke the callback right away.
        if (session.OrganisationOptions.Count() == 0) {
            return await this.SendErrorCallback(session, SelectOrganisationErrorCode.NoOptions, cancellationToken);
        }

        // If there is only one option; invoke the callback right away.
        if (session.OrganisationOptions.Count() == 1) {
            // Invalidate the session since it is being handled now.
            await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
                SessionKey = sessionKey,
            }, cancellationToken);

            var selectedOrganisation = (await getOrganisationById.InvokeAsync(new() {
                OrganisationId = session.OrganisationOptions.First().Id
            }, cancellationToken)).Organisation;

            if (selectedOrganisation is null) {
                // The organisation does not exist; maybe it was deleted.
                return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection, cancellationToken);
            }

            var selectionPayload = this.RemapSelectedOrganisationToCallbackData(session, selectedOrganisation);
            return await this.SendCallback(session, selectionPayload, cancellationToken);
        }

        // Present prompt to the user.
        return this.View(new SelectOrganisationViewModel {
            SignOutUrl = this.Url.Action(
                action: "SignOut",
                values: new { clientId, sessionKey }
            ),
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
            AllowCancel = session.AllowCancel,
        });
    }

    [HttpPost]
    [Route("{clientId}/{sessionKey}")]
    public async Task<IActionResult> PostIndex(
        string clientId,
        string sessionKey,
        SelectOrganisationViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey, cancellationToken);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        if (viewModel.Cancel == "1") {
            if (session.AllowCancel) {
                return await this.SendCancelCallback(session, cancellationToken);
            }
            else {
                return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection, cancellationToken);
            }
        }

        if (viewModel.SelectedOrganisationId is null) {
            // Present prompt to the user with error.
            this.ModelState.AddModelError(nameof(SelectOrganisationViewModel.SelectedOrganisationId), "Select one organisation.");
            return this.View("Index", new SelectOrganisationViewModel {
                SignOutUrl = this.Url.Action(
                    action: "SignOut",
                    values: new { clientId, sessionKey }
                ),
                Prompt = session.Prompt,
                OrganisationOptions = session.OrganisationOptions,
                AllowCancel = session.AllowCancel,
            });
        }

        await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
            SessionKey = sessionKey,
        }, cancellationToken);

        bool didUserSelectOptionThatWasPresented = session.OrganisationOptions.Any(
            option => option.Id == viewModel.SelectedOrganisationId);
        if (!didUserSelectOptionThatWasPresented) {
            return await this.HandleInvalidSessionAsync(clientId, cancellationToken);
        }

        var selectedOrganisation = (await getOrganisationById.InvokeAsync(new() {
            OrganisationId = (Guid)viewModel.SelectedOrganisationId,
        }, cancellationToken)).Organisation;
        if (selectedOrganisation is null) {
            return await this.SendErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection, cancellationToken);
        }

        var selectionCallbackData = this.RemapSelectedOrganisationToCallbackData(session, selectedOrganisation);
        return await this.SendCallback(session, selectionCallbackData, cancellationToken);
    }

    [HttpGet]
    [Route("{clientId}/{sessionKey}/sign-out")]
    public async Task<IActionResult> SignOut(
        string clientId,
        string sessionKey,
        CancellationToken cancellationToken = default)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey, cancellationToken);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        return await this.SendCallback(session, new SelectOrganisationCallbackSignOut {
            Type = PayloadTypeConstants.SignOut,
            UserId = session.UserId,
        }, cancellationToken);
    }

    private async Task<IActionResult> SendCallback(
        SelectOrganisationSessionData session,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        string payloadJson = JsonSerializer.Serialize(payload, jsonOptions);
        string payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

        var signingResponse = await createDigitalSignatureForPayload.InvokeAsync(new() {
            Payload = payloadBase64,
        }, cancellationToken);

        return this.View("Callback", new CallbackViewModel {
            CallbackUrl = session.CallbackUrl,
            PayloadType = ((SelectOrganisationCallback)payload).Type,
            PayloadData = payloadBase64,
            DigitalSignature = signingResponse.Signature.Signature,
            PublicKeyId = signingResponse.Signature.KeyId,
        });
    }

    private Task<IActionResult> SendErrorCallback(
        SelectOrganisationSessionData session,
        SelectOrganisationErrorCode errorCode,
        CancellationToken cancellationToken = default)
    {
        return this.SendCallback(session, new SelectOrganisationCallbackError {
            Type = PayloadTypeConstants.Error,
            UserId = session.UserId,
            Code = errorCode,
        }, cancellationToken);
    }

    private Task<IActionResult> SendCancelCallback(
        SelectOrganisationSessionData session,
        CancellationToken cancellationToken = default)
    {
        return this.SendCallback(session, new SelectOrganisationCallbackCancel {
            Type = PayloadTypeConstants.Cancel,
            UserId = session.UserId,
        }, cancellationToken);
    }

    private sealed record GetSessionResult()
    {
        public SelectOrganisationSessionData? Session { get; init; } = null;
        public IActionResult? RedirectActionResult { get; init; } = null;
    }

    private async Task<GetSessionResult> GetSessionAsync(
        string clientId,
        string sessionKey,
        CancellationToken cancellationToken = default)
    {
        var session = (await getSelectOrganisationSessionByKey.InvokeAsync(new() {
            SessionKey = sessionKey,
        }, cancellationToken)).SessionData;

        if (session is null) {
            // Redirect when session does not exist.
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(clientId, cancellationToken),
            };
        }

        if (clientId != session.ClientId) {
            // User has tampered with the clientId parameter of the URL.
            await invalidateSelectOrganisationSessionRequest.InvokeAsync(new() {
                SessionKey = sessionKey,
            }, cancellationToken);
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(session.ClientId, cancellationToken),
            };
        }

        return new GetSessionResult { Session = session };
    }

    private SelectOrganisationCallbackSelection RemapSelectedOrganisationToCallbackData(
        SelectOrganisationSessionData session,
        OrganisationModel selectedOrganisation)
    {
        return new() {
            Type = PayloadTypeConstants.Selection,
            DetailLevel = session.DetailLevel,
            UserId = session.UserId,
            Selection = (OrganisationDetails)mapper.Map(
                source: selectedOrganisation,
                sourceType: selectedOrganisation.GetType(),
                destinationType: OrganisationDetails.ResolveType(session.DetailLevel)
            ),
        };
    }

    private async Task<IActionResult> HandleInvalidSessionAsync(
        string? clientId,
        CancellationToken cancellationToken = default)
    {
        return this.View("InvalidSessionError", new InvalidSessionViewModel {
            ReturnUrl = await this.GetServiceHomeUrlAsync(clientId, cancellationToken),
        });
    }

    private async Task<Uri> GetServiceHomeUrlAsync(
        string? clientId,
        CancellationToken cancellationToken = default)
    {
        var returnUrl = applicationOptionsAccessor.Value.ServicesUrl;

        if (!string.IsNullOrEmpty(clientId)) {
            var response = await getApplicationByClientId.InvokeAsync(new() {
                ClientId = clientId,
            }, cancellationToken);
            if (response.Application is not null) {
                returnUrl = response.Application.ServiceHomeUrl;
            }
        }

        return returnUrl;

    }
}
