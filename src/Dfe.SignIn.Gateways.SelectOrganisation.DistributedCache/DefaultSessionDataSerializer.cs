using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;

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
    public SelectOrganisationSessionData Deserialize(string sessionDataJson)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(sessionDataJson, nameof(sessionDataJson));

        var data = JsonSerializer.Deserialize<SelectOrganisationSessionData>(sessionDataJson, JsonSerializerOptions);
        return data ?? throw new JsonException("Invalid object.");
    }
}
