using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Distributed;

namespace Dfe.SignIn.Gateways.DistributedCache;

/// <summary>
/// Unique values that identify the <see cref="IDistributedCache"/> instances that are to
/// be used with dependency injection.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DistributedCacheKeys
{
    /// <summary>
    /// A unique value that identifies a general cache.
    /// </summary>
    public const string GeneralCache = "d47d8b06-0afd-44e7-8bed-4cb7c7c1e8af";

    /// <summary>
    /// A unique value that identifies the application session cache.
    /// </summary>
    public const string SessionCache = "1a8844bc-665f-4c5b-a219-c73a010db3c9";

    /// <summary>
    /// A unique value that identifies the Entra External ID token cache.
    /// </summary>
    public const string EntraTokenCache = "ae03be65-ed22-43ca-8fce-fe147a29cc39";

    /// <summary>
    /// A unique value that identifies the "Select organisation" session cache.
    /// </summary>
    public const string SelectOrganisationSessions = "ee86309b-8f3a-4eac-943c-dc14a3c60343";

    /// <summary>
    /// A unique value that identifies the interaction request cache.
    /// </summary>
    public const string InteractionRequests = "58370e88-02ac-4ea9-95de-3d30c0c8b8df";
}
