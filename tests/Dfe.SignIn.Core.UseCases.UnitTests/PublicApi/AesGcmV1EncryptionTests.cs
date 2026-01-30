
using System.Security.Cryptography;
using System.Text;
using Dfe.SignIn.Core.UseCases.PublicApi;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

[TestClass]
public sealed class AesGcmV1EncryptionTests
{

    private static byte[] HelloWorldString => Encoding.UTF8.GetBytes("Hello, World!");
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes(new string('F', 32));

    [TestMethod]
    public void Encrypt_KeyTooShort_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => {
            var key = RandomNumberGenerator.GetBytes(16);
            AesGcmV1EncryptionProvider.Encrypt(key, HelloWorldString);
        });

        Assert.Contains($"Invalid encryption key length 16.", ex.Message);
    }

    [TestMethod]
    public void Decrypt_KeyTooShort_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => {
            var key = RandomNumberGenerator.GetBytes(16);
            AesGcmV1EncryptionProvider.Decrypt(key, HelloWorldString);
        });

        Assert.Contains($"Invalid encryption key length 16.", ex.Message);
    }

    [TestMethod]
    public void EncryptDecrypt_ReturnsOriginalText()
    {
        var encrypted = AesGcmV1EncryptionProvider.Encrypt(EncryptionKey, HelloWorldString);
        var decrypted = AesGcmV1EncryptionProvider.Decrypt(EncryptionKey, encrypted);
        var decryptedString = Encoding.UTF8.GetString(decrypted);

        Assert.AreEqual("Hello, World!", decryptedString);
    }

    [TestMethod]
    public void ShouldDecrypt_NodeEncryptedValue()
    {
        var decrypted = AesGcmV1EncryptionProvider.Decrypt(
            Encoding.UTF8.GetBytes(new string('F', 32)),
            Convert.FromBase64String("DLCKHuDD2Y4NM04TzrvUu/26QBuuTDaPK8iD7u2eM1MoNYW0QnIUUvdllqGj42mD6hXXDQ=="));
        var decryptedString = Encoding.UTF8.GetString(decrypted);

        Assert.AreEqual("Node says: Hello, World!", decryptedString);
    }

    [TestMethod]
    public void Decrypt_InvalidCipherText_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => AesGcmV1EncryptionProvider.Decrypt(EncryptionKey, Encoding.UTF8.GetBytes("shortdata")));
        Assert.Contains("Invalid encrypted data.", ex.Message);
    }

    [TestMethod]
    public void Decrypt_ModifiedCipherText_ThrowsCryptographicException()
    {
        Assert.ThrowsExactly<AuthenticationTagMismatchException>(() => {
            var encrypted = AesGcmV1EncryptionProvider.Encrypt(EncryptionKey, Encoding.UTF8.GetBytes("Sensitive data"));
            encrypted[^1] ^= 0xFF;

            AesGcmV1EncryptionProvider.Decrypt(EncryptionKey, encrypted);
        });
    }

    [TestMethod]
    public void Decrypt_EncryptedDataTooShort_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => {
            var tooShort = new byte[10];
            AesGcmV1EncryptionProvider.Decrypt(EncryptionKey, tooShort);
        });

        Assert.Contains("Invalid encrypted data", ex.Message);
    }
}
