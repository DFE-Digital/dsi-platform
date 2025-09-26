using System.Text.Json;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Gateways.DistributedCache.Serialization;

/// <inheritdoc/>
public sealed class DefaultCacheEntrySerializer : ICacheEntrySerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <inheritdoc/>
    public string Serialize<T>(T entry) where T : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(entry, nameof(entry));

        return JsonSerializer.Serialize(entry, JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public T Deserialize<T>(string entryJson) where T : class
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(entryJson, nameof(entryJson));

        var data = JsonSerializer.Deserialize<T>(entryJson, JsonSerializerOptions);
        return data ?? throw new JsonException("Invalid object.");
    }
}
