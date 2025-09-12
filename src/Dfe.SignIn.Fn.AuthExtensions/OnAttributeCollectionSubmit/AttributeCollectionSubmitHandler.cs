using Dfe.SignIn.Fn.AuthExtensions.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionSubmit;

public sealed class AttributeCollectionSubmitHandler(ILogger<AttributeCollectionSubmitHandler> logger)
{
    [Function("OnAttributeCollectionSubmit")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        var @event = await request.ReadFromJsonAsync<AttributeCollectionSubmitEvent?>()
            ?? throw new InvalidOperationException("Missing event body.");

        logger.LogInformation(
            "Received authentication event. CorrelationId: {CorrelationId}",
            @event.Data.AuthenticationContext.CorrelationId
        );

        @event.Validate();

        string surname = @event.Data.UserSignUpInfo.Attributes.Surname.Value;
        string givenName = @event.Data.UserSignUpInfo.Attributes.GivenName.Value;

        return ResponseAction(new ModifyAttributeValuesAction {
            Attributes = new() {
                [UserAttributeConstants.DisplayName] = $"{surname.ToUpperInvariant()}, {givenName}",
            },
        });
    }

    private static OkObjectResult ResponseAction(IResponseAction action)
    {
        return new OkObjectResult(new ResponseObject {
            Data = new AttributeCollectionSubmitEventResponseData {
                Actions = [action],
            },
        });
    }
}
