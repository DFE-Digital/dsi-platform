namespace Dfe.SignIn.Gateways.DistributedCache.Serialization;

/// <summary>
/// Represents a service that serializes and deserializes cache entries.
/// </summary>
public interface ICacheEntrySerializer
{
    /// <summary>
    /// Serialize cache entry data to a JSON encoded string.
    /// </summary>
    /// <typeparam name="T">The type of cache entry.</typeparam>
    /// <param name="entry">The data of a cache entry.</param>
    /// <returns>
    ///   <para>The JSON encoded representation of the cache entry data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="entry"/> is null.</para>
    /// </exception>
    string Serialize<T>(T entry) where T : class;

    /// <summary>
    /// Deserialize a typed cache entry from a JSON encoded string.
    /// </summary>
    /// <typeparam name="T">The type of cache entry.</typeparam>
    /// <param name="entryJson">The JSON encoded representation of the cache entry data.</param>
    /// <returns>
    ///   <para>The deserialized data of a cache entry.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="entryJson"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="entryJson"/> is an empty string.</para>
    /// </exception>
    T Deserialize<T>(string entryJson) where T : class;
}
