using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionStart;

public sealed record AttributeCollectionStartEvent
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("data")]
    public required AttributeCollectionStartEventCalloutData Data { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.authenticationEvent.attributeCollectionStart") {
            throw new InvalidOperationException($"Invalid event type '{this.Type}'.");
        }

        this.Data.Validate();
    }
}

public sealed record AttributeCollectionStartEventCalloutData
{
    [JsonPropertyName("@odata.type")]
    public required string Type { get; init; }

    [JsonPropertyName("authenticationContext")]
    public required AuthenticationContext AuthenticationContext { get; init; }

    [JsonPropertyName("userSignUpInfo")]
    public required UserSignUpInfo UserSignUpInfo { get; init; }

    public void Validate()
    {
        if (this.Type != "microsoft.graph.onAttributeCollectionStartCalloutData") {
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
    [JsonPropertyName("identities")]
    public required List<IdentityData> Identities { get; init; }

    public void Validate()
    {
        if (this.Identities.Count == 0) {
            throw new InvalidOperationException("Has no identities.");
        }
        if (this.Identities.Count != 1) {
            throw new InvalidOperationException("Has multiple identities.");
        }
    }
}

public sealed record IdentityData
{
    [JsonPropertyName("signInType")]
    public required string SignInType { get; init; }

    [JsonPropertyName("issuer")]
    public required string Issuer { get; init; }

    [JsonPropertyName("issuerAssignedId")]
    public required string IssuerAssignedId { get; init; }
}

public static class SignInTypes
{
    public const string EmailAddress = "emailAddress";
}
