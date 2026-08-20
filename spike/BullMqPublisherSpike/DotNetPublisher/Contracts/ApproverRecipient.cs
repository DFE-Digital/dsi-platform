using System.Text.Json.Serialization;

namespace BullMqPublisherSpike.Contracts;

public sealed record ApproverRecipient
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }
}
