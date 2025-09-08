using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions.OnTokenIssuanceStart;

public sealed record TokenIssuanceStartEventResponseData
{
    [JsonPropertyName("@odata.type")]
    public string DataType { get; } = "microsoft.graph.onTokenIssuanceStartResponseData";

    [JsonPropertyName("actions")]
    public required List<IResponseAction> Actions { get; init; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(ProvideClaimsForTokenAction))]
public interface IResponseAction
{
}

public sealed record ProvideClaimsForTokenAction : IResponseAction
{
    [JsonPropertyName("@odata.type")]
    public string DataType => "microsoft.graph.provideClaimsForToken";

    [JsonPropertyName("claims")]
    public required Dictionary<string, string> Claims { get; init; }
}
