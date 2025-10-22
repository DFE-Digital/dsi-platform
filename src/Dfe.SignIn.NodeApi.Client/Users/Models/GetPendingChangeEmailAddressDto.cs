using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record GetPendingChangeEmailAddressDto
{
    [JsonPropertyName("email")]
    public required string NewEmailAddress { get; init; }

    [JsonPropertyName("code")]
    public required string VerificationCode { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAtUtc { get; init; }
}
