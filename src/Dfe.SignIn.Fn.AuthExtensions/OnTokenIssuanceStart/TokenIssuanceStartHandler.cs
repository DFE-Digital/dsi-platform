using Dfe.SignIn.Base.Framework;
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

        var checkLinkedResponse = await interaction.DispatchAsync(
            new AutoLinkEntraUserToDsiRequest {
                EntraUserId = @event.Data.AuthenticationContext.User.Id,
                EmailAddress = @event.Data.AuthenticationContext.User.Mail,
                GivenName = @event.Data.AuthenticationContext.User.GivenName,
                Surname = @event.Data.AuthenticationContext.User.Surname,
            }
        ).To<AutoLinkEntraUserToDsiResponse>();

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
