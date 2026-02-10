using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.Web.SelectOrganisation.Models;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.SelectOrganisation.Controllers;

/// <summary>
/// The controller for selecting an organisation.
/// </summary>
[Route("")]
public sealed class SelectOrganisationController(
    IOptions<PlatformOptions> platformOptionsAccessor,
    IInteractionDispatcher interaction
) : Controller
{
    private SelectOrganisationViewModel PrepareViewModel(
        string clientId, string sessionKey, SelectOrganisationSessionData session)
    {
        return new SelectOrganisationViewModel {
            SignOutUrl = this.Url.Action(
                action: nameof(SignOut),
                values: new { clientId, sessionKey }
            ),
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
            AllowCancel = session.AllowCancel,
        };
    }

    [HttpGet("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(
        [FromRoute] string clientId, [FromRoute] string sessionKey)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        // If there are no options; invoke the callback right away.
        if (!session.OrganisationOptions.Any()) {
            return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.NoOptions);
        }

        // If there is only one option; invoke the callback right away.
        if (session.OrganisationOptions.Count() == 1) {
            return await this.HandleOnlyOneOption(sessionKey, session);
        }

        return this.View(this.PrepareViewModel(clientId, sessionKey, session));
    }

    private async Task<IActionResult> HandleOnlyOneOption(string sessionKey, SelectOrganisationSessionData session)
    {
        // Invalidate the session since it is being handled now.
        await interaction.DispatchAsync(
            new InvalidateSelectOrganisationSessionRequest {
                SessionKey = sessionKey,
            }
        ).To<InvalidateSelectOrganisationSessionResponse>();

        try {
            var selectedOrganisation = (await interaction.DispatchAsync(
                new GetOrganisationByIdRequest {
                    OrganisationId = session.OrganisationOptions.First().Id
                }
            ).To<GetOrganisationByIdResponse>()).Organisation;

            await interaction.DispatchAsync(
                new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.SelectOrganisation,
                    EventName = AuditSelectOrganisationEventNames.Selection,
                    Message = $"Automatically selected only option '{selectedOrganisation.Name}'",
                    UserId = session.UserId,
                    OrganisationId = selectedOrganisation.Id,
                }
            );

            return this.RedirectToSelectionCallback(session, selectedOrganisation.Id);
        }
        catch (OrganisationNotFoundException) {
            // The organisation does not exist; maybe it was deleted.
            return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
        }
    }

    [HttpPost("{clientId}/{sessionKey}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostIndex(
        [FromRoute] string clientId, [FromRoute] string sessionKey,
        SelectOrganisationViewModel viewModel)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        if (viewModel.CancelAction == "1") {
            if (session.AllowCancel) {
                return this.RedirectToCancelCallback(session);
            }
            else {
                return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
            }
        }

        if (viewModel.SelectedOrganisationIdInput is null) {
            this.ModelState.AddModelError(
                nameof(SelectOrganisationViewModel.SelectedOrganisationIdInput),
                "Select one organisation."
            );
        }

        if (!this.ModelState.IsValid) {
            return this.View("Index", this.PrepareViewModel(clientId, sessionKey, session));
        }

        await interaction.DispatchAsync(
            new InvalidateSelectOrganisationSessionRequest {
                SessionKey = sessionKey,
            }
        );

        bool didUserSelectOptionThatWasPresented = session.OrganisationOptions.Any(
            option => option.Id == viewModel.SelectedOrganisationIdInput);
        if (!didUserSelectOptionThatWasPresented) {
            return await this.HandleInvalidSessionAsync(clientId);
        }

        try {
            var selectedOrganisation = (await interaction.DispatchAsync(
                new GetOrganisationByIdRequest {
                    OrganisationId = (Guid)viewModel.SelectedOrganisationIdInput!,
                }
            ).To<GetOrganisationByIdResponse>()).Organisation;

            await interaction.DispatchAsync(
                new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.SelectOrganisation,
                    EventName = AuditSelectOrganisationEventNames.Selection,
                    Message = $"Selected organisation '{selectedOrganisation.Name}'",
                    UserId = session.UserId,
                    OrganisationId = selectedOrganisation.Id,
                }
            );

            return this.RedirectToSelectionCallback(session, selectedOrganisation.Id);
        }
        catch (OrganisationNotFoundException) {
            return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
        }
    }

    [HttpGet("{clientId}/{sessionKey}/sign-out")]
    public async Task<IActionResult> SignOut(string clientId, string sessionKey)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        return this.RedirectToSignOutCallback(session);
    }

    private RedirectResult RedirectToSelectionCallback(
        SelectOrganisationSessionData session, Guid selection)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Selection}&{CallbackParamNames.Selection}={selection}");
    }

    private RedirectResult RedirectToErrorCallback(SelectOrganisationSessionData session, string errorCode)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Error}&{CallbackParamNames.ErrorCode}={errorCode}");
    }

    private RedirectResult RedirectToCancelCallback(SelectOrganisationSessionData session)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Cancel}");
    }

    private RedirectResult RedirectToSignOutCallback(SelectOrganisationSessionData session)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.SignOut}");
    }

    private sealed record GetSessionResult
    {
        public SelectOrganisationSessionData? Session { get; init; } = null;
        public IActionResult? RedirectActionResult { get; init; } = null;
    }

    private async Task<GetSessionResult> GetSessionAsync(string clientId, string sessionKey)
    {
        var session = (await interaction.DispatchAsync(
            new GetSelectOrganisationSessionByKeyRequest {
                SessionKey = sessionKey,
            }
        ).To<GetSelectOrganisationSessionByKeyResponse>()).SessionData;

        if (session is null) {
            // Redirect when session does not exist.
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(clientId),
            };
        }

        if (clientId != session.ClientId) {
            // User has tampered with the clientId parameter of the URL.
            await interaction.DispatchAsync(
                new InvalidateSelectOrganisationSessionRequest {
                    SessionKey = sessionKey,
                }
            );
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(session.ClientId),
            };
        }

        return new GetSessionResult { Session = session };
    }

    private async Task<IActionResult> HandleInvalidSessionAsync(string? clientId)
    {
        return this.View("InvalidSessionError", new InvalidSessionViewModel {
            ReturnUrl = await this.GetServiceHomeUrlAsync(clientId)
                ?? platformOptionsAccessor.Value.ServicesUrl,
        });
    }

    private async Task<Uri?> GetServiceHomeUrlAsync(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId)) {
            return null;
        }

        try {
            var response = await interaction.DispatchAsync(
                new GetApplicationByClientIdRequest {
                    ClientId = clientId,
                }
            ).To<GetApplicationByClientIdResponse>();
            return response.Application.ServiceHomeUrl;
        }
        catch (ApplicationNotFoundException) {
            return null;
        }
    }
}
