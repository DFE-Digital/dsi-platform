using System.Text.Json.Serialization;

namespace Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionSubmit;

public sealed record AttributeCollectionSubmitEventResponseData
{
    [JsonPropertyName("@odata.type")]
    public string DataType { get; } = "microsoft.graph.onAttributeCollectionSubmitResponseData";

    [JsonPropertyName("actions")]
    public required List<IResponseAction> Actions { get; init; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(ModifyAttributeValuesAction))]
[JsonDerivedType(typeof(ShowValidationErrorAction))]
public interface IResponseAction
{
}

public sealed record ModifyAttributeValuesAction : IResponseAction
{
    [JsonPropertyName("@odata.type")]
    public string DataType => "microsoft.graph.attributeCollectionSubmit.modifyAttributeValues";

    [JsonPropertyName("attributes")]
    public required Dictionary<string, object> Attributes { get; init; }
}

public sealed record ShowValidationErrorAction : IResponseAction
{
    [JsonPropertyName("@odata.type")]
    public string DataType => "microsoft.graph.attributeCollectionSubmit.showValidationError";

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("attributeErrors")]
    public required Dictionary<string, string> AttributeErrors { get; init; }
}
