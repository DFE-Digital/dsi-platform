using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.InternalApi.Contracts;

/// <summary>
/// Represents the outcome of an interaction request, encapsulating either a
/// successful response or an error.
/// </summary>
/// <typeparam name="TResponse">The expected type of the response model.</typeparam>
public sealed record InteractionResponse<TResponse>
    where TResponse : class
{
    /// <summary>
    /// Creates a successful <see cref="InteractionResponse{TResponse}"/> from
    /// the specified response model.
    /// </summary>
    /// <param name="response">The response model.</param>
    /// <returns>
    ///   <para>An <see cref="InteractionResponse{TResponse}"/> instance containing
    ///   the provided response data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="response"/> is null.</para>
    /// </exception>
    public static InteractionResponse<TResponse> FromResponse(TResponse response)
    {
        ExceptionHelpers.ThrowIfArgumentNull(response, nameof(response));

        return new() {
            Content = new() {
                Type = response.GetType().FullName ?? throw new InvalidOperationException(),
                Data = response,
            },
        };
    }

    /// <summary>
    /// Creates a failed <see cref="InteractionResponse{TResponse}"/> from the
    /// specified exception.
    /// </summary>
    /// <param name="exception">The exception that occurred during the interaction.</param>
    /// <param name="exceptionSerializer">The serializer used to convert the exception to JSON.</param>
    /// <returns>
    ///   <para>An <see cref="InteractionResponse{TResponse}"/> instance containing
    ///   the serialized exception details.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="exception"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="exceptionSerializer"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the exception type name cannot be determined.</para>
    /// </exception>
    public static InteractionResponse<TResponse> FromException(
        Exception exception, IExceptionJsonSerializer exceptionSerializer)
    {
        ExceptionHelpers.ThrowIfArgumentNull(exception, nameof(exception));
        ExceptionHelpers.ThrowIfArgumentNull(exceptionSerializer, nameof(exceptionSerializer));

        return new() {
            Exception = exceptionSerializer.SerializeExceptionToJson(exception),
        };
    }

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
    public InteractionResponseContent<TResponse>? Content { get; init; }

    /// <summary>
    /// Contains exception details if the interaction failed; otherwise, null.
    /// </summary>
    public JsonElement? Exception { get; init; }
}

/// <summary>
/// Content included in response to a successful interaction request.
/// </summary>
public sealed record InteractionResponseContent<TResponse>
    where TResponse : class
{
    /// <summary>
    /// The name or identifier of the response content type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The deserialized response data, encoded in JSON format.
    /// </summary>
    public required TResponse Data { get; init; }
}
