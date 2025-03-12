using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApiClient.Organisations.Models;

internal sealed record OrganisationByIdDto() : OrganisationDto
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }
}