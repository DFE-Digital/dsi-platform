namespace Dfe.SignIn.Base.Framework.Caching;

/// <summary>
/// Constant values for <see cref="ICacheableRequest"/>'s.
/// </summary>
public static class CacheableRequestConstants
{
    /// <summary>
    /// A default cache key that is suitable for request types that do not have any
    /// form of parameterisation.
    /// </summary>
    public const string DefaultCacheKey = "default";
}

/// <summary>
/// Indicates that a request can be cached.
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// Gets the unique key representing the cache entry for this request.
    /// </summary>
    string CacheKey { get; }
}
