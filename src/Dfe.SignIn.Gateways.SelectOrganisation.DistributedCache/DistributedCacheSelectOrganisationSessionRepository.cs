using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache;

/// <summary>
/// A service that retrieves "select organisation" sessions from using distributed cache.
/// </summary>
/// <param name="cache">The distributed cache.</param>
/// <param name="serializer">A service for serializing session data.</param>
/// <exception cref="ArgumentNullException">
///   <para>If <paramref name="cache"/> is null.</para>
///   <para>- or -</para>
///   <para>If <paramref name="serializer"/> is null.</para>
/// </exception>
public sealed class DistributedCacheSelectOrganisationSessionRepository(
    [FromKeyedServices(SelectOrganisationConstants.CacheStoreKey)] IDistributedCache cache,
    ISessionDataSerializer serializer
) : ISelectOrganisationSessionRepository
{
    /// <inheritdoc/>
    public async Task<SelectOrganisationSessionData?> RetrieveAsync(string sessionKey)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(sessionKey, nameof(sessionKey));

        string? sessionDataJson = await cache.GetStringAsync(sessionKey);
        if (sessionDataJson is null) {
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
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(sessionKey, nameof(sessionKey));
        ExceptionHelpers.ThrowIfArgumentNull(sessionData, nameof(sessionData));

        string sessionDataJson = serializer.Serialize(sessionData);
        await cache.SetStringAsync(sessionKey, sessionDataJson, new DistributedCacheEntryOptions {
            AbsoluteExpiration = sessionData.Expires,
        });
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync(string sessionKey)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(sessionKey, nameof(sessionKey));

        await cache.RemoveAsync(sessionKey);
    }
}
