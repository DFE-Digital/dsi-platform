using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Fn.AuthExtensions.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionStart;

public sealed class AttributeCollectionStartHandler(
    ILogger<AttributeCollectionStartHandler> logger,
    IInteractionDispatcher interaction)
{
    [Function("OnAttributeCollectionStart")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        var @event = await request.ReadFromJsonAsync<AttributeCollectionStartEvent?>()
            ?? throw new InvalidOperationException("Missing event body.");

        logger.LogInformation(
            "Received authentication event. CorrelationId: {CorrelationId}",
            @event.Data.AuthenticationContext.CorrelationId
        );

        @event.Validate();

        var checkEmailResponse = await interaction.DispatchAsync(
            new CheckIsBlockedEmailAddressRequest {
                EmailAddress = @event.Data.UserSignUpInfo.Identities[0].IssuerAssignedId,
            }
        ).To<CheckIsBlockedEmailAddressResponse>();

        if (checkEmailResponse.IsBlocked) {
            logger.LogInformation("Email address was blocked by policy requirements.");
            return ResponseAction(new ShowBlockPageAction {
                Message = MessageConstants.BlockedEmailAddress,
            });
        }

        return ResponseAction(new ContinueWithDefaultBehaviorAction());
    }

    private static OkObjectResult ResponseAction(IResponseAction action)
    {
        return new OkObjectResult(new ResponseObject {
            Data = new AttributeCollectionStartEventResponseData {
                Actions = [action],
            },
        });
    }
}
