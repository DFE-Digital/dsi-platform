using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record AssignServicesFromInvitationRequestDto
{
    [JsonPropertyName("userId")]
    public required Guid UserId { get; init; }
}

internal sealed record AssignServicesFromInvitationResponseDto
{
}
