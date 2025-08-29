using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Applications.Interactions;
using Dfe.SignIn.Core.InternalModels.Organisations.Interactions;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
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
                action: "SignOut",
                values: new { clientId, sessionKey }
            ),
            Prompt = session.Prompt,
            OrganisationOptions = session.OrganisationOptions,
            AllowCancel = session.AllowCancel,
        };
    }

    [HttpGet("{clientId}/{sessionKey}")]
    public async Task<IActionResult> Index(
        string clientId, string sessionKey, CancellationToken cancellationToken = default)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey, cancellationToken);
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
            // Invalidate the session since it is being handled now.
            await interaction.DispatchAsync(
                new InvalidateSelectOrganisationSessionRequest {
                    SessionKey = sessionKey,
                }, cancellationToken
            ).To<InvalidateSelectOrganisationSessionResponse>();

            var selectedOrganisation = (await interaction.DispatchAsync(
                new GetOrganisationByIdRequest {
                    OrganisationId = session.OrganisationOptions.First().Id
                }, cancellationToken
            ).To<GetOrganisationByIdResponse>()).Organisation;

            if (selectedOrganisation is null) {
                // The organisation does not exist; maybe it was deleted.
                return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
            }

            return this.RedirectToSelectionCallback(session, selectedOrganisation.Id);
        }

        return this.View(this.PrepareViewModel(clientId, sessionKey, session));
    }

    [HttpPost("{clientId}/{sessionKey}")]
    public async Task<IActionResult> PostIndex(
        string clientId, string sessionKey, SelectOrganisationViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        var sessionResult = await this.GetSessionAsync(clientId, sessionKey, cancellationToken);
        if (sessionResult.Session is null) {
            return sessionResult.RedirectActionResult!;
        }
        var session = sessionResult.Session;

        if (viewModel.CancelInput == "1") {
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
            }, cancellationToken
        );

        bool didUserSelectOptionThatWasPresented = session.OrganisationOptions.Any(
            option => option.Id == viewModel.SelectedOrganisationIdInput);
        if (!didUserSelectOptionThatWasPresented) {
            return await this.HandleInvalidSessionAsync(clientId, cancellationToken);
        }

        var selectedOrganisation = (await interaction.DispatchAsync(
            new GetOrganisationByIdRequest {
                OrganisationId = (Guid)viewModel.SelectedOrganisationIdInput!,
            }, cancellationToken
        ).To<GetOrganisationByIdResponse>()).Organisation;
        if (selectedOrganisation is null) {
            return this.RedirectToErrorCallback(session, SelectOrganisationErrorCode.InvalidSelection);
        }

        return this.RedirectToSelectionCallback(session, selectedOrganisation.Id);
    }

    [HttpGet("{clientId}/{sessionKey}/sign-out")]
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

        return this.RedirectToSignOutCallback(session);
    }

    private IActionResult RedirectToSelectionCallback(
        SelectOrganisationSessionData session, Guid selection)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Selection}&{CallbackParamNames.Selection}={selection}");
    }

    private IActionResult RedirectToErrorCallback(SelectOrganisationSessionData session, string errorCode)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Error}&{CallbackParamNames.ErrorCode}={errorCode}");
    }

    private IActionResult RedirectToCancelCallback(SelectOrganisationSessionData session)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.Cancel}");
    }

    private IActionResult RedirectToSignOutCallback(SelectOrganisationSessionData session)
    {
        return this.Redirect($"{session.CallbackUrl}&{CallbackParamNames.Type}={CallbackTypes.SignOut}");
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
        var session = (await interaction.DispatchAsync(
            new GetSelectOrganisationSessionByKeyRequest {
                SessionKey = sessionKey,
            }, cancellationToken
        ).To<GetSelectOrganisationSessionByKeyResponse>()).SessionData;

        if (session is null) {
            // Redirect when session does not exist.
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(clientId, cancellationToken),
            };
        }

        if (clientId != session.ClientId) {
            // User has tampered with the clientId parameter of the URL.
            await interaction.DispatchAsync(
                new InvalidateSelectOrganisationSessionRequest {
                    SessionKey = sessionKey,
                }, cancellationToken
            );
            return new GetSessionResult {
                RedirectActionResult = await this.HandleInvalidSessionAsync(session.ClientId, cancellationToken),
            };
        }

        return new GetSessionResult { Session = session };
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
        var returnUrl = platformOptionsAccessor.Value.ServicesUrl;

        if (!string.IsNullOrEmpty(clientId)) {
            var response = await interaction.DispatchAsync(
                new GetApplicationByClientIdRequest {
                    ClientId = clientId,
                }, cancellationToken
            ).To<GetApplicationByClientIdResponse>();
            if (response.Application?.ServiceHomeUrl is not null) {
                returnUrl = response.Application.ServiceHomeUrl;
            }
        }

        return returnUrl;
    }
}
