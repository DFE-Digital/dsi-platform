using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Fn.AuthExtensions.OnTokenIssuanceStart;

public sealed class TokenIssuanceStartHandler(
    ILogger<TokenIssuanceStartHandler> logger,
    IInteractionDispatcher interaction)
{
    [Function("OnTokenIssuanceStart")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        var @event = await request.ReadFromJsonAsync<TokenIssuanceStartEvent?>()
            ?? throw new InvalidOperationException("Missing event body.");

        logger.LogInformation(
            "Received authentication event. CorrelationId: {CorrelationId}",
            @event.Data.AuthenticationContext.CorrelationId
        );

        @event.Validate();

        AutoLinkEntraUserToDsiResponse checkLinkedResponse;
        try {
            checkLinkedResponse = await interaction.DispatchAsync(
                new AutoLinkEntraUserToDsiRequest {
                    EntraUserId = @event.Data.AuthenticationContext.User.Id,
                    EmailAddress = @event.Data.AuthenticationContext.User.Mail.Trim(),
                    FirstName = @event.Data.AuthenticationContext.User.GivenName.Trim(),
                    LastName = @event.Data.AuthenticationContext.User.Surname.Trim(),
                }
            ).To<AutoLinkEntraUserToDsiResponse>();
        }
        catch (Exception ex) when (ex is CannotLinkInactiveUserException
            or CannotCreateNewUserException
            or UserAlreadyLinkedToEntraAccountException
            or EntraAccountAlreadyLinkedToDifferentUserException) {
            // CannotLinkInactiveUserException is a permanent failure requiring account
            // reactivation: retrying (e.g. on the user's next sign-in attempt) will hit the
            // same conflict every time. CannotCreateNewUserException is not reliably
            // permanent — it may occur because of the registration race window in
            // CreateUserUseCase's pre-check, in which case the user's next sign-in attempt
            // will find the now-committed row by email via
            // AutoLinkEntraUserToDsiUseCase.LinkToExistingDsiUserAsync and self-heal. Other
            // linking-conflict exceptions indicate a genuine conflict that will not resolve
            // without support intervention.
            string outcomeGuidance = ex switch {
                CannotLinkInactiveUserException => "permanent failure requiring account reactivation",
                CannotCreateNewUserException => "may self-heal on the user's next sign-in if this was a registration race, but investigate if recurring for the same account",
                _ => "permanent failure requiring support investigation",
            };
            // CreateUserUseCase's pre-check, in which case the user's next sign-in attempt
            // will find the now-committed row by email via
            // AutoLinkEntraUserToDsiUseCase.LinkToExistingDsiUserAsync and self-heal. Audit
            // either way so support has visibility, then rethrow — this extension point has
            // no way to return a custom error response to Entra, only claims for a
            // successful token.
            string outcomeGuidance = ex is CannotLinkInactiveUserException
                ? "permanent failure requiring account reactivation"
                : "may self-heal on the user's next sign-in if this was a registration race, but investigate if recurring for the same account";

            try {
                await interaction.DispatchAsync(new WriteToAuditRequest {
                    EventCategory = AuditEventCategoryNames.Auth,
                    EventName = AuditAuthEventNames.LinkFailed,
                    Message = $"Failed to link Entra account {@event.Data.AuthenticationContext.User.Id} ({@event.Data.AuthenticationContext.User.Mail.Trim()}) to a DfE Sign-In user: {ex.GetType().Name} ({outcomeGuidance}).",
                    WasFailure = true,
                });
            }
            catch (Exception auditEx) {
                // Never let an audit-infrastructure failure mask the original exception
                // being handled here — log it and continue to the rethrow below.
                logger.LogError(auditEx, "Failed to write link-failure audit entry.");
            }

            throw;
        }

        return ResponseAction(new ProvideClaimsForTokenAction {
            Claims = new() {
                [DsiClaimTypes.UserId] = checkLinkedResponse.UserId.ToString(),
            },
        });
    }

    private static OkObjectResult ResponseAction(IResponseAction action)
    {
        return new OkObjectResult(new ResponseObject {
            Data = new TokenIssuanceStartEventResponseData {
                Actions = [action],
            },
        });
    }
}
