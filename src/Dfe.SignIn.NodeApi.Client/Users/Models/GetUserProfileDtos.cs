using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record GetUserProfileDto
{
    [JsonPropertyName("isEntra")]
    public required bool IsEntra { get; init; }

    [JsonPropertyName("isInternalUser")]
    public required bool IsInternalUser { get; init; }

    [JsonPropertyName("given_name")]
    public required string FirstName { get; init; }

    [JsonPropertyName("family_name")]
    public required string LastName { get; init; }

    [JsonPropertyName("job_title")]
    public required string JobTitle { get; init; }

    [JsonPropertyName("email")]
    public required string EmailAddress { get; init; }
}
