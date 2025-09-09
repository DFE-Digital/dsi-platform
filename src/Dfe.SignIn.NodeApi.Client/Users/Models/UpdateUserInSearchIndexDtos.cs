using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record UpdateUserInSearchIndexRequestDto()
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
}
