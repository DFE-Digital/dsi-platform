using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.SelectOrganisation.SessionData.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.SessionData;

/// <summary>
/// A service that retrieves "select organisation" sessions from using distributed cache.
/// </summary>
/// <param name="optionsAccessor">Configured options.</param>
/// <param name="cache">The distributed cache.</param>
/// <param name="serializer">A service for serializing session data.</param>
/// <exception cref="ArgumentNullException">
///   <para>If <paramref name="optionsAccessor"/> is null.</para>
///   <para>- or -</para>
///   <para>If <paramref name="cache"/> is null.</para>
///   <para>- or -</para>
///   <para>If <paramref name="serializer"/> is null.</para>
/// </exception>
public sealed class DistributedCacheSelectOrganisationSessionRepository(
    IOptions<SelectOrganisationSessionCacheOptions> options,
    IDistributedCache cache,
    ISessionDataSerializer serializer
) : ISelectOrganisationSessionRepository
{
    /// <inheritdoc/>
    public async Task<SelectOrganisationSessionData?> RetrieveAsync(string sessionKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));

        string cacheKey = $"{options.Value.CacheKeyPrefix}{sessionKey}";
        string? sessionDataJson = await cache.GetStringAsync(cacheKey);
        if (sessionDataJson == null) {
            return null;
        }

        var sessionData = serializer.Deserialize(sessionDataJson);
        if (DateTime.UtcNow > sessionData.Expires) {
            return null;
        }

        return sessionData;
    }

    /// <inheritdoc/>
    public async Task StoreAsync(string sessionKey, SelectOrganisationSessionData sessionData)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));
        ArgumentNullException.ThrowIfNull(sessionData, nameof(sessionData));

        string cacheKey = $"{options.Value.CacheKeyPrefix}{sessionKey}";
        string sessionDataJson = serializer.Serialize(sessionData);
        await cache.SetStringAsync(cacheKey, sessionDataJson);
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync(string sessionKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));

        string cacheKey = $"{options.Value.CacheKeyPrefix}{sessionKey}";

        await cache.RemoveAsync(cacheKey);
    }
}
