using System.Text.Json;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.InternalApi.Contracts;

/// <summary>
/// Represents the outcome of a successful interaction request.
/// </summary>
/// <typeparam name="TResponse">The expected type of the response model.</typeparam>
public sealed record InteractionResponse<TResponse>
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

/// <summary>
/// Represents the outcome of a successful interaction request.
/// </summary>
public sealed record InteractionResponse
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

/// <summary>
/// Represents the outcome of a failed interaction request.
/// </summary>
public sealed record FailedInteractionResponse
{
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
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="exception"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="exceptionSerializer"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the exception type name cannot be determined.</para>
    /// </exception>
    public static FailedInteractionResponse FromException(
        Exception exception, IExceptionJsonSerializer exceptionSerializer)
    {
        ExceptionHelpers.ThrowIfArgumentNull(exception, nameof(exception));
        ExceptionHelpers.ThrowIfArgumentNull(exceptionSerializer, nameof(exceptionSerializer));

        return new() {
            Exception = exceptionSerializer.SerializeExceptionToJson(exception),
        };
    }

    /// <summary>
    /// Contains exception details if the interaction failed; otherwise, null.
    /// </summary>
    public JsonElement Exception { get; init; }
}
