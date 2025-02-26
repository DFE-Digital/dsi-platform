using Dfe.SignIn.SelectOrganisation.Data.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Data.DistributedCache;

/// <summary>
/// A service that retrieves "select organisation" sessions from using distributed cache.
/// </summary>
public sealed class DistributedCacheSelectOrganisationSessionRetriever : ISelectOrganisationSessionRetriever
{
    private readonly SelectOrganisationSessionCacheOptions options;
    private readonly IDistributedCache cache;
    private readonly ISessionDataSerializer serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheSelectOrganisationSessionRetriever"/> class.
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
    public DistributedCacheSelectOrganisationSessionRetriever(
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
    public async Task<SelectOrganisationSessionData?> RetrieveSessionAsync(string sessionKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionKey, nameof(sessionKey));

        string cacheKey = $"{this.options.CacheKeyPrefix}{sessionKey}";
        string? sessionDataJson = await this.cache.GetStringAsync(cacheKey);
        if (sessionDataJson == null) {
            return null;
        }

        var sessionData = this.serializer.Deserialize(sessionDataJson);
        if (DateTime.UtcNow > sessionData.Expires) {
            return null;
        }

        return sessionData;
    }
}
