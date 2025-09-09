using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.NodeApi.Client.Users.Models;

internal sealed record GetUserStatusResponseDto()
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("status")]
    public required AccountStatus Status { get; init; }
}
