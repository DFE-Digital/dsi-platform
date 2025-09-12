using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionSubmit;

public sealed record AttributeCollectionSubmitEvent
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("data")]
    public required AttributeCollectionSubmitEventCalloutData Data { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.authenticationEvent.attributeCollectionSubmit") {
            throw new InvalidOperationException($"Invalid event type '{this.Type}'.");
        }

        this.Data.Validate();
    }
}

public sealed record AttributeCollectionSubmitEventCalloutData
{
    [JsonPropertyName("@odata.type")]
    public required string Type { get; init; }

    [JsonPropertyName("authenticationContext")]
    public required AuthenticationContext AuthenticationContext { get; init; }

    [JsonPropertyName("userSignUpInfo")]
    public required UserSignUpInfo UserSignUpInfo { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.onAttributeCollectionSubmitCalloutData") {
            throw new InvalidOperationException($"Invalid callout data type '{this.Type}'.");
        }

        this.UserSignUpInfo.Validate();
    }
}

public sealed record AuthenticationContext
{
    [JsonPropertyName("correlationId")]
    public required string CorrelationId { get; init; }
}

public sealed record UserSignUpInfo
{
    [JsonPropertyName("attributes")]
    public required UserSignUpInfoAttributes Attributes { get; init; }

    public void Validate()
    {
        this.Attributes.Validate();
    }
}

public sealed record UserSignUpInfoAttributes
{
    [JsonPropertyName("givenName")]
    public required UserSignUpInfoAttribute<string> GivenName { get; init; }

    [JsonPropertyName("surname")]
    public required UserSignUpInfoAttribute<string> Surname { get; init; }

    private const string StringDirectoryAttributeType = "microsoft.graph.stringDirectoryAttributeValue";

    public void Validate()
    {
        if (this.GivenName?.Type != StringDirectoryAttributeType) {
            throw new ValidationException("Invalid value for attribute 'givenName'.");
        }
        if (this.Surname?.Type != StringDirectoryAttributeType) {
            throw new ValidationException("Invalid value for attribute 'surname'.");
        }
    }
}

public sealed record UserSignUpInfoAttribute<TValue>
{
    [JsonPropertyName("@odata.type")]
    public required string Type { get; init; }

    [JsonPropertyName("value")]
    public required TValue Value { get; init; }

    [JsonPropertyName("attributeType")]
    public required string AttributeType { get; init; }
}
