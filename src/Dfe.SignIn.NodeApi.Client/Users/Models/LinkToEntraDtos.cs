using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record LinkToEntraRequestDto
{
    [JsonPropertyName("entraOid")]
    public required Guid EntraOid { get; init; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }
}

internal sealed record LinkToEntraResponseDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
}
