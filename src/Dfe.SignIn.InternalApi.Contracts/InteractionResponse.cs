using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.InternalApi.Contracts;

/// <summary>
/// Represents the outcome of an interaction request, encapsulating either a
/// successful response or an error.
/// </summary>
public sealed record InteractionResponse
{
    /// <summary>
    /// Indicates whether the interaction completed successfully.
    /// </summary>
    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Content))]
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success => this.Exception is null;

    /// <summary>
    /// Contains the response content if the interaction was successful; otherwise, null.
    /// </summary>
    public InteractionResponseContent? Content { get; init; }

    /// <summary>
    /// Contains exception details if the interaction failed; otherwise, null.
    /// </summary>
    public JsonElement? Exception { get; init; }
}

/// <summary>
/// Content included in response to a successful interaction request.
/// </summary>
public sealed record InteractionResponseContent
{
    /// <summary>
    /// The name or identifier of the response content type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The deserialized response data, encoded in JSON format.
    /// </summary>
    public required JsonElement Data { get; init; }
}
