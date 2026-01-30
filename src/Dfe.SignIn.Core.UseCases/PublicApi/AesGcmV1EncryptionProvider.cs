using System.Security.Cryptography;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Provides static methods to encrypt and decrypt data using AES-256-GCM (Version 1).
/// </summary>
internal static class AesGcmV1EncryptionProvider
{
    private const int IvLength = 12;
    private const int TagLength = 16;

    /// <summary>
    /// Encrypts the given plaintext using AES-256-GCM with the provided encryption key.
    /// </summary>
    /// <param name="encryptionKey">A 32-byte key used for encryption.</param>
    /// <param name="plainTextBytes">The plaintext byte array to encrypt.</param>
    /// <returns>
    ///   <para>A byte array containing the concatenated IV, authentication tag, and ciphertext.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>Thrown if the provided <paramref name="encryptionKey"/> is not exactly 32 bytes.</para>
    /// </exception>
    public static byte[] Encrypt(byte[] encryptionKey, byte[] plainTextBytes)
    {
        if (encryptionKey.Length != 32) {
            throw new ArgumentException($"Invalid encryption key length {encryptionKey.Length}.");
        }

        byte[] iv = RandomNumberGenerator.GetBytes(IvLength);
        byte[] cipherText = new byte[plainTextBytes.Length];
        byte[] tag = new byte[TagLength];

        using (var aesGcm = new AesGcm(encryptionKey, TagLength)) {
            aesGcm.Encrypt(iv, plainTextBytes, cipherText, tag);
        }

        // Combine IV + Tag + CipherText
        byte[] combined = new byte[IvLength + TagLength + cipherText.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, IvLength);
        Buffer.BlockCopy(tag, 0, combined, IvLength, TagLength);
        Buffer.BlockCopy(cipherText, 0, combined, IvLength + TagLength, cipherText.Length);

        return combined;
    }

    /// <summary>
    /// Decrypts a Base64-encoded encrypted string produced by <see cref="Encrypt"/>.
    /// </summary>
    /// <param name="encryptionKey">A 32-byte key used for decryption (must match the encryption key).</param>
    /// <param name="encryptedBytes">The encrypted byte array to decrypt.</param>
    /// <returns>
    ///   <para>A byte array containing the plaintext (UTF-8 encoded).</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>Thrown if the encryption key is invalid, or if the encrypted data is malformed
    ///   e.g. too short to contain IV + tag + ciphertext.
    ///   </para>
    /// </exception>
    public static byte[] Decrypt(byte[] encryptionKey, byte[] encryptedBytes)
    {
        if (encryptionKey.Length != 32) {
            throw new ArgumentException($"Invalid encryption key length {encryptionKey.Length}.");
        }

        if (encryptedBytes.Length < IvLength + TagLength) {
            throw new ArgumentException("Invalid encrypted data.");
        }

        byte[] iv = new byte[IvLength];
        byte[] tag = new byte[TagLength];
        byte[] cipherText = new byte[encryptedBytes.Length - IvLength - TagLength];

        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IvLength);
        Buffer.BlockCopy(encryptedBytes, IvLength, tag, 0, TagLength);
        Buffer.BlockCopy(encryptedBytes, IvLength + TagLength, cipherText, 0, cipherText.Length);

        byte[] plainBytes = new byte[cipherText.Length];

        using (var aesGcm = new AesGcm(encryptionKey, TagLength)) {
            aesGcm.Decrypt(iv, cipherText, tag, plainBytes);
        }

        return plainBytes;
    }
}
