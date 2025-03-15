using System.Text.Json;
using Dfe.SignIn.Core.Models.SelectOrganisation;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;

/// <inheritdoc/>
public sealed class DefaultSessionDataSerializer : ISessionDataSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <inheritdoc/>
    public string Serialize(SelectOrganisationSessionData sessionData)
    {
        ArgumentNullException.ThrowIfNull(sessionData, nameof(sessionData));

        return JsonSerializer.Serialize(sessionData, JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public SelectOrganisationSessionData Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        var data = JsonSerializer.Deserialize<SelectOrganisationSessionData>(json, JsonSerializerOptions);
        return data
            ?? throw new JsonException("Invalid object.");
    }
}
