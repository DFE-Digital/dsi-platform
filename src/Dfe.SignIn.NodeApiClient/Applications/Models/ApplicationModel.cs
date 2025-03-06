using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApiClient.Applications.Models;

public sealed record ApplicationModel()
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required RelyingPartyModel RelyingParty { get; init; }
}

public sealed record RelyingPartyModel()
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; init; }

    [JsonPropertyName("api_secret")]
    public string? ApiSecret { get; init; } = null;

    [JsonPropertyName("service_home")]
    public string? ServiceHome { get; init; }

    [JsonPropertyName("redirect_uris")]
    public required string[] RedirectUris { get; init; } = [];

    [JsonPropertyName("post_logout_redirect_uris")]
    public required string[] PostLogoutRedirectUris { get; init; } = [];

    [JsonPropertyName("grant_types")]
    public required string[] GrantTypes { get; init; } = [];

    [JsonPropertyName("response_types")]
    public required string[] ResponseTypes { get; init; } = [];

    public RelyingPartyParamModel? Params { get; init; }
}
public sealed record RelyingPartyParamModel()
{
    public required string Header { get; init; }
    public required string HeaderMessage { get; init; }
    public required Guid ServiceId { get; init; }
}
