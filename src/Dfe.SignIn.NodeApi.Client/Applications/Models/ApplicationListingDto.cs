using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Applications.Models;

internal sealed record ApplicationListingDto()
{
    [JsonPropertyName("services")]
    public ApplicationDto[] Services { get; init; } = [];
}
