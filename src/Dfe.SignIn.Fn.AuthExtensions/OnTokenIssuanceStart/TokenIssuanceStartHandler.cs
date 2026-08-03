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
        catch (Exception ex) when (ex is CannotLinkInactiveUserException or CannotCreateNewUserException) {
            // Permanent failure: retrying (e.g. on the user's next sign-in attempt) will hit
            // the same conflict every time, unlike transient errors which self-heal. Audit it
            // so support has visibility, then rethrow — this extension point has no way to
            // return a custom error response to Entra, only claims for a successful token.
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkFailed,
                Message = $"Failed to link Entra account {@event.Data.AuthenticationContext.User.Id} ({@event.Data.AuthenticationContext.User.Mail.Trim()}) to a DfE Sign-In user: {ex.GetType().Name}",
                WasFailure = true,
            });
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
