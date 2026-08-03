using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Fn.AuthExtensions.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionSubmit;

public sealed class AttributeCollectionSubmitHandler(
    ILogger<AttributeCollectionSubmitHandler> logger,
    IValidator<ChangeNameRequest> changeNameValidator)
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

        string givenName = @event.Data.UserSignUpInfo.Attributes.GivenName.Value;
        string surname = @event.Data.UserSignUpInfo.Attributes.Surname.Value;

        var validationResult = this.ValidateNameInputs(givenName, surname);
        if (validationResult is not null) {
            return validationResult;
        }

        return ResponseAction(new ModifyAttributeValuesAction {
            Attributes = new() {
                [UserAttributeConstants.DisplayName] = $"{surname.ToUpperInvariant()}, {givenName}",
            },
        });
    }

    private static readonly Dictionary<string, string> AttributeNameMappings = new() {
        [nameof(ChangeNameRequest.FirstName)] = "givenName",
        [nameof(ChangeNameRequest.LastName)] = "surname",
    };

    private OkObjectResult? ValidateNameInputs(string givenName, string surname)
    {
        var validationResults = new List<ValidationResult>();

        var changeNameRequest = new ChangeNameRequest {
            UserId = Guid.NewGuid(), // ID is not needed for validation.
            FirstName = givenName,
            LastName = surname,
        };

        var fvResult = changeNameValidator.Validate(changeNameRequest);

        if (!fvResult.IsValid) {
            foreach (var error in fvResult.Errors) {
                validationResults.Add(new ValidationResult(error.ErrorMessage, [error.PropertyName]));
            }
        }

        bool isValid = fvResult.IsValid;

        if (isValid) {
            return null;
        }

        var attributeErrors = fvResult.Errors
            .Where(failure => AttributeNameMappings.ContainsKey(failure.PropertyName))
            .GroupBy(failure => AttributeNameMappings[failure.PropertyName])
            .ToDictionary(
                group => group.Key,
                group => group.First().ErrorMessage ?? "Invalid value."
            );

        return ResponseAction(new ShowValidationErrorAction {
            Message = "Please fix the below errors to proceed.",
            AttributeErrors = attributeErrors
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
