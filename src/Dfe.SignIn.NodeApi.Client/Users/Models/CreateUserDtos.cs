using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record CreateUserRequestDto
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }

    [JsonPropertyName("password")]
    public required string? Password { get; init; }

    [JsonPropertyName("entraOid")]
    public required Guid EntraOid { get; init; }
}

internal sealed record CreateUserResponseDto
{
    [JsonPropertyName("sub")]
    public required Guid Id { get; init; }
}
