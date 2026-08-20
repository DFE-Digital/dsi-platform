using System.Text.Json.Serialization;

namespace BullMqPublisherSpike.Contracts;

public sealed record ApproverAccessRequestPayload
{
    [JsonPropertyName("recipients")]
    public required IReadOnlyList<ApproverRecipient> Recipients { get; init; }

    [JsonPropertyName("orgName")]
    public required string OrgName { get; init; }

    [JsonPropertyName("userName")]
    public required string UserName { get; init; }

    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; init; }

    [JsonPropertyName("orgId")]
    public required string OrgId { get; init; }

    [JsonPropertyName("requestId")]
    public required string RequestId { get; init; }
}
