using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Defines a service for shallow serialization and deserialization of exceptions
/// to and from JSON.
/// </summary>
/// <remarks>
///   <para>This serializer does not include reflected type metadata or inner
///   exceptions in the output.</para>
/// </remarks>
public interface IExceptionJsonSerializer
{
    /// <summary>
    /// Serializes an exception to a JSON encoded string.
    /// </summary>
    /// <param name="exception">The exception to serialize.</param>
    /// <returns>
    ///   <para>A <see cref="JsonElement"/> containing the serialized exception data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="exception"/> is null.</para>
    /// </exception>
    JsonElement SerializeExceptionToJson(Exception exception);

    /// <summary>
    /// Reconstructs an <see cref="Exception"/> instance from its JSON representation.
    /// </summary>
    /// <remarks>
    ///   <para>Returns an exception of type <see cref="UnexpectedException"/> when the
    ///   exception type cannot be determined.</para>
    /// </remarks>
    /// <param name="jsonElement">The JSON-encoded exception data.</param>
    /// <returns>
    ///   <para>The deserialized exception instance.</para>
    /// </returns>
    Exception DeserializeExceptionFromJson(JsonElement jsonElement);
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
    private readonly JsonSerializerOptions options = jsonOptionsAccessor.Get(JsonHelperExtensions.StandardOptionsKey);

    /// <inheritdoc/>
    public JsonElement SerializeExceptionToJson(Exception exception)
    {
        ExceptionHelpers.ThrowIfArgumentNull(exception, nameof(exception));

        return JsonSerializer.SerializeToElement(exception, this.options);
    }

    /// <inheritdoc/>
    public Exception DeserializeExceptionFromJson(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind != JsonValueKind.Object) {
            return new UnexpectedException("Unknown exception type.");
        }

        return JsonSerializer.Deserialize<Exception>(jsonElement, this.options)!;
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

        if (!rootElement.TryGetProperty("data", out var dataElement)) {
            dataElement = rootElement;
        }

        // Get message from exception.
        string? message = null;
        if (dataElement.TryGetProperty("message", out var messageElement)) {
            message = messageElement.GetString();
        }

        // Create the exception instance.
        try {
            var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

            // Deserialize exception details.
            foreach (var property in ExceptionReflectionHelpers.GetSerializableExceptionProperties(exceptionType)) {
                string propertyName = namingPolicy.ConvertName(property.Name);
                if (dataElement.TryGetProperty(propertyName, out var propertyElement)) {
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

internal sealed class ValidationResultJsonConverter : JsonConverter<ValidationResult>
{
    private static string ConvertNameToPascalCase(string name)
    {
        return string.IsNullOrEmpty(name)
            ? name
            : char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    /// <inheritdoc/>
    public override ValidationResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var namingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDocument.RootElement;

        string? errorMessage = null;
        if (rootElement.TryGetProperty(namingPolicy.ConvertName(nameof(ValidationResult.ErrorMessage)), out var errorMessageElement)) {
            errorMessage = errorMessageElement.GetString();
        }

        string[]? memberNames = null;
        if (rootElement.TryGetProperty(namingPolicy.ConvertName(nameof(ValidationResult.MemberNames)), out var memberNamesElement)) {
            memberNames = memberNamesElement.Deserialize<string[]>()
                ?.Select(ConvertNameToPascalCase).ToArray();
        }

        return new ValidationResult(errorMessage, memberNames ?? []);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ValidationResult validationResult, JsonSerializerOptions options)
    {
        var namingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        writer.WriteStartObject();

        writer.WriteString(namingPolicy.ConvertName(nameof(ValidationResult.ErrorMessage)), validationResult.ErrorMessage);

        writer.WriteStartArray(namingPolicy.ConvertName(nameof(ValidationResult.MemberNames)));
        foreach (string memberName in validationResult.MemberNames) {
            writer.WriteStringValue(namingPolicy.ConvertName(memberName));
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
