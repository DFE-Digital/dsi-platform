using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Errors.Models;

internal sealed record ErrorMessageDto
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
