using System.Security.Cryptography;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;

namespace Dfe.SignIn.PublicApiClient.PublicApiSigning;

internal interface IPublicKeyCache
{
    /// <summary>
    /// Retrieve public key from cache.
    /// </summary>
    /// <remarks>
    ///   <para>Cache is refreshed:</para>
    ///   <list type="bullet">
    ///     <item>Periodically.</item>
    ///     <item>When a potentially new public key is encountered.</item>
    ///   </list>
    /// </remarks>
    /// <param name="keyId">Unique identifier of key.</param>
    /// <returns>
    ///   <para>The public key if found; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="keyId"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="keyId"/> is an empty string.</para>
    /// </exception>
    Task<PublicKeyCacheEntry?> GetPublicKeyAsync(string keyId);
}

internal sealed class PublicKeyCacheEntry(WellKnownPublicKey key, RSA rsa)
{
    public WellKnownPublicKey Key => key;
    public RSA RSA => rsa;
}
