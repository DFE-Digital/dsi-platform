using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Organisations.Models;

internal sealed record OrganisationByIdDto : OrganisationDto
{
    [JsonPropertyName("status")]
    public required int Status { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("type")]
    public string? EstablishmentType { get; init; }
}
