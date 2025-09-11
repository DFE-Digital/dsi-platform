using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record AssignOrganisationsFromInvitationRequestDto
{
    [JsonPropertyName("user_id")]
    public required Guid UserId { get; init; }
}

internal sealed record AssignOrganisationsFromInvitationResponseDto
{
}
