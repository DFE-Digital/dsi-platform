using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.PublicApi.Contracts.Internal;

/// <summary>
/// Resolves difference between Node and C# hmac
/// </summary>
/// <exclude/>
public static class HmacKeyNormalizer
{
    /// <summary>
    /// Ensures that the provided key meets the minimum length requirement for HMAC-SHA256 (32 bytes).
    /// If the key is shorter, it will be zero-padded to 32 bytes.
    /// </summary>
    /// <param name="keyBytes">The original key as a byte array.</param>
    /// <returns>A byte array that is at least 32 bytes long.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided keyBytes is null.</exception>
    public static byte[] NormalizeHmacSha256Key(byte[] keyBytes)
    {
        ExceptionHelpers.ThrowIfArgumentNull(keyBytes, nameof(keyBytes));

        if (keyBytes.Length < 32) {
            var paddedKey = new byte[32];
            Array.Copy(keyBytes, 0, paddedKey, 0, keyBytes.Length);
            return paddedKey;
        }

        return keyBytes;
    }
}
