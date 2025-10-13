using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record ConvertInvitationToUserRequestDto
{
    [JsonPropertyName("entraOid")]
    public required Guid EntraOid { get; init; }
}

internal sealed record ConvertInvitationToUserResponseDto
{
    [JsonPropertyName("sub")]
    public required Guid Id { get; init; }
}
