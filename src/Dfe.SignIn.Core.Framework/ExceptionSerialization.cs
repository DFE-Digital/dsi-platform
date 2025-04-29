using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// A service that performs a shallow serialization of exceptions to or from JSON
/// encoded strings.
/// </summary>
/// <remarks>
///   <para>Reflected type information and inner exceptions are not serialized.</para>
/// </remarks>
public interface IExceptionJsonSerializer
{
    /// <summary>
    /// Serializes an exception to a JSON encoded string.
    /// </summary>
    /// <param name="exception">The exception instance.</param>
    /// <returns>
    ///   <para>The JSON encoded string.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="exception"/> is null.</para>
    /// </exception>
    string SerializeExceptionToJson(Exception exception);

    /// <summary>
    /// Deserializes an exception from a JSON encoded string.
    /// </summary>
    /// <remarks>
    ///   <para>Returns an exception of type <see cref="UnexpectedException"/> when the
    ///   exception type cannot be determined.</para>
    /// </remarks>
    /// <param name="exception">The exception instance.</param>
    /// <returns>
    ///   <para>The deserialized exception instance.</para>
    /// </returns>
    Exception DeserializeExceptionFromJson(string? json);
}

/// <summary>
/// The default implementation of a service that serializes exceptions to or from JSON
/// encoded strings using <see cref="JsonSerializer"/>.
/// </summary>
/// <param name="jsonOptionsAccessor">Provides access to JSON serializer options.</param>
public sealed class DefaultExceptionJsonSerializer(
    IOptionsMonitor<JsonSerializerOptions> jsonOptionsAccessor
) : IExceptionJsonSerializer
{
    /// <inheritdoc/>
    public string SerializeExceptionToJson(Exception exception)
    {
        ExceptionHelpers.ThrowIfArgumentNull(exception, nameof(exception));

        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        return JsonSerializer.Serialize(exception, jsonOptions);
    }

    /// <inheritdoc/>
    public Exception DeserializeExceptionFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) {
            return new UnexpectedException("Unknown exception type.");
        }

        var jsonOptions = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);
        return JsonSerializer.Deserialize<Exception>(json!, jsonOptions)!;
    }
}

internal sealed class ExceptionJsonConverter : JsonConverter<Exception>
{
    /// <inheritdoc/>
    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var namingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDocument.RootElement;

        // Determine the type of exception.
        Type? exceptionType = null;
        if (rootElement.TryGetProperty("type", out var typeElement)) {
            string typeName = typeElement.GetString()!;
            exceptionType = ExceptionReflectionHelpers.GetExceptionTypeByFullName(typeName);
        }
        exceptionType ??= typeof(UnexpectedException);

        // Get message from exception.
        string? message = null;
        if (rootElement.TryGetProperty("message", out var messageElement)) {
            message = messageElement.GetString();
        }

        // Create the exception instance.
        try {
            var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

            // Deserialize exception details.
            foreach (var property in ExceptionReflectionHelpers.GetSerializableExceptionProperties(exceptionType)) {
                string propertyName = namingPolicy.ConvertName(property.Name);
                if (rootElement.TryGetProperty(propertyName, out var propertyElement)) {
                    var value = propertyElement.Deserialize(property.PropertyType, options);
                    if (value is not null) {
                        property.SetValue(exception, value);
                    }
                }
            }

            return exception;
        }
        catch (MissingMethodException) {
            // Was unable to find a public constructor for the exception type.
            return new UnexpectedException(message!);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Exception exception, JsonSerializerOptions options)
    {
        var namingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        writer.WriteStartObject();
        var exceptionType = exception.GetType();

        writer.WriteString(namingPolicy.ConvertName("Type"), exceptionType.FullName);

        if (!string.IsNullOrWhiteSpace(exception.Message)) {
            writer.WriteString(namingPolicy.ConvertName(nameof(Exception.Message)), exception.Message);
        }

        foreach (var property in ExceptionReflectionHelpers.GetSerializableExceptionProperties(exceptionType)) {
            var propertyValue = property.GetValue(exception, null);
            if (propertyValue is not null) {
                writer.WritePropertyName(namingPolicy.ConvertName(property.Name));
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}
