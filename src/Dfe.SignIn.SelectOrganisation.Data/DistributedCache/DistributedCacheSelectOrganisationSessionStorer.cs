using Dfe.SignIn.SelectOrganisation.Data.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Data.DistributedCache;

/// <summary>
/// A service that stores and invalidates "select organisation" sessions using distributed cache.
/// </summary>
public sealed class DistributedCacheSelectOrganisationSessionStorer : ISelectOrganisationSessionStorer
{
    private readonly SelectOrganisationSessionCacheOptions options;
    private readonly IDistributedCache cache;
    private readonly ISessionDataSerializer serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheSelectOrganisationSessionStorer"/> class.
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
    public DistributedCacheSelectOrganisationSessionStorer(
        IOptions<SelectOrganisationSessionCacheOptions> optionsAccessor,
        IDistributedCache cache,
        ISessionDataSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor, nameof(optionsAccessor));
        ArgumentNullException.ThrowIfNull(cache, nameof(cache));
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        this.options = optionsAccessor.Value;
        this.cache = cache;
        this.serializer = serializer;
    }

    /// <inheritdoc/>
    public async Task StoreSessionAsync(string sessionKey, SelectOrganisationSessionData sessionData)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));
        ArgumentNullException.ThrowIfNull(sessionData, nameof(sessionData));

        string cacheKey = $"{this.options.CacheKeyPrefix}{sessionKey}";
        string sessionDataJson = this.serializer.Serialize(sessionData);
        await this.cache.SetStringAsync(cacheKey, sessionDataJson);
    }

    /// <inheritdoc/>
    public async Task InvalidateSessionAsync(string sessionKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));

        string cacheKey = $"{this.options.CacheKeyPrefix}{sessionKey}";

        await this.cache.RemoveAsync(cacheKey);
    }
}
