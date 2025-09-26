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
    /// A unique value that identifies the "Select organisation" session cache.
    /// </summary>
    public const string SelectOrganisationSessions = "ee86309b-8f3a-4eac-943c-dc14a3c60343";
}
