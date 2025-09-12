using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record GetInvitationResponseDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("isCompleted")]
    public required bool IsCompleted { get; init; }
}
