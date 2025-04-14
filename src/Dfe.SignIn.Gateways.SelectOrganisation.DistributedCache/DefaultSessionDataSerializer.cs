using System.Text.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;

/// <inheritdoc/>
public sealed class DefaultSessionDataSerializer : ISessionDataSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <inheritdoc/>
    public string Serialize(SelectOrganisationSessionData sessionData)
    {
        ExceptionHelpers.ThrowIfArgumentNull(sessionData, nameof(sessionData));

        return JsonSerializer.Serialize(sessionData, JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public SelectOrganisationSessionData Deserialize(string json)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(json, nameof(json));

        var data = JsonSerializer.Deserialize<SelectOrganisationSessionData>(json, JsonSerializerOptions);
        return data
            ?? throw new JsonException("Invalid object.");
    }
}
