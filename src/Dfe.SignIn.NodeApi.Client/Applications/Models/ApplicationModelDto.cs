using System.Text.Json.Serialization;

namespace Dfe.SignIn.NodeApi.Client.Applications.Models;

internal sealed record ApplicationModelDto()
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsExternalService { get; init; }
    public bool IsIdOnlyService { get; init; }
    public bool IsHiddenService { get; init; }
    public required RelyingPartyModelDto RelyingParty { get; init; }
}

internal sealed record RelyingPartyModelDto()
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; init; }

    [JsonPropertyName("api_secret")]
    public string? ApiSecret { get; init; } = null;

    [JsonPropertyName("service_home")]
    public required string ServiceHome { get; init; }

    [JsonPropertyName("redirect_uris")]
    public string[] RedirectUris { get; init; } = [];

    [JsonPropertyName("post_logout_redirect_uris")]
    public string[] PostLogoutRedirectUris { get; init; } = [];

    [JsonPropertyName("grant_types")]
    public string[] GrantTypes { get; init; } = [];

    [JsonPropertyName("response_types")]
    public string[] ResponseTypes { get; init; } = [];

    public RelyingPartyParamModelDto? Params { get; init; }
}
internal sealed record RelyingPartyParamModelDto()
{
    public string? Header { get; init; }
    public string? HeaderMessage { get; init; }
    public Guid? ServiceId { get; init; }
}
