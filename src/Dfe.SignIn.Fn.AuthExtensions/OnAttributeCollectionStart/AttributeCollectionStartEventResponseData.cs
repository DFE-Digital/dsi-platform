using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionStart;

public sealed record AttributeCollectionStartEventResponseData
{
    [JsonPropertyName("@odata.type")]
    public string DataType { get; } = "microsoft.graph.onAttributeCollectionStartResponseData";

    [JsonPropertyName("actions")]
    public required List<IResponseAction> Actions { get; init; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(ContinueWithDefaultBehaviorAction))]
[JsonDerivedType(typeof(ShowBlockPageAction))]
public interface IResponseAction
{
}

public sealed record ContinueWithDefaultBehaviorAction : IResponseAction
{
    [JsonPropertyName("@odata.type")]
    public string DataType => "microsoft.graph.attributeCollectionStart.continueWithDefaultBehavior";
}

public sealed record ShowBlockPageAction : IResponseAction
{
    [JsonPropertyName("@odata.type")]
    public string DataType => "microsoft.graph.attributeCollectionStart.showBlockPage";

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
