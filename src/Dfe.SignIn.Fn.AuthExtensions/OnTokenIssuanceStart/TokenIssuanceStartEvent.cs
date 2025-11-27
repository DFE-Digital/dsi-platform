using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Contracts;

namespace Dfe.SignIn.Fn.AuthExtensions.OnTokenIssuanceStart;

public sealed record TokenIssuanceStartEvent
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("data")]
    public required TokenIssuanceStartEventCalloutData Data { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.authenticationEvent.tokenIssuanceStart") {
            throw new InvalidOperationException($"Invalid event type '{this.Type}'.");
        }

        this.Data.Validate();
    }
}

public sealed record TokenIssuanceStartEventCalloutData
{
    [JsonPropertyName("@odata.type")]
    public required string Type { get; init; }

    [JsonPropertyName("tenantId")]
    public required Guid TenantId { get; init; }

    [JsonPropertyName("authenticationContext")]
    public required AuthenticationContext AuthenticationContext { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.onTokenIssuanceStartCalloutData") {
            throw new InvalidOperationException($"Invalid callout data type '{this.Type}'.");
        }

        this.AuthenticationContext.Validate();
    }
}

public sealed record AuthenticationContext
{
    [JsonPropertyName("correlationId")]
    public required string CorrelationId { get; init; }

    [JsonPropertyName("user")]
    public required UserContext User { get; init; }

    public void Validate()
    {
        this.User.Validate();
    }
}

public sealed record UserContext
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("mail")]
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string Mail { get; init; }

    [JsonPropertyName("displayName")]
    [MinLength(1)]
    public required string DisplayName { get; init; }

    [JsonPropertyName("givenName")]
    [MinLength(1)]
    public required string GivenName { get; init; }

    [JsonPropertyName("surname")]
    [MinLength(1)]
    public required string Surname { get; init; }

    [JsonPropertyName("userPrincipalName")]
    public required string UserPrincipalName { get; init; }

    [JsonPropertyName("userType")]
    public required string UserType { get; init; }

    public void Validate()
    {
        var validationContext = new ValidationContext(this);
        Validator.ValidateObject(this, validationContext, validateAllProperties: true);
    }
}
