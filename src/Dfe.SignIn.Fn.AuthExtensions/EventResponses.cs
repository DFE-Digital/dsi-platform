using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions;

public sealed record ResponseObject
{
    [JsonPropertyName("data")]
    public required object Data { get; init; }
}
