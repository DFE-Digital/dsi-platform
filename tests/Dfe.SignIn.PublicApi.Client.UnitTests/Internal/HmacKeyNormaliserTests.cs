using Dfe.SignIn.PublicApi.Client.Internal;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.Internal;

[TestClass]
public sealed class HmacKeyPaddingTests
{
    [TestMethod]
    public void Key_Less_Than_32_Bytes_Should_Be_Padded()
    {
        byte[] keyBytes = new byte[16]; // 16-byte key (too short)
        new Random().NextBytes(keyBytes);

        byte[] result = HmacKeyNormalizer.NormalizeHmacSha256Key(keyBytes);

        Assert.AreEqual(32, result.Length);
        CollectionAssert.AreEqual(keyBytes, result[..16]); // Ensure original bytes are unchanged
        CollectionAssert.AreEqual(new byte[16], result[16..]); // Ensure padding is zeroed
    }

    [TestMethod]
    public void Key_Exactly_32_Bytes_Should_Remain_Unchanged()
    {
        byte[] keyBytes = new byte[32];
        new Random().NextBytes(keyBytes);
        byte[] expectedKey = (byte[])keyBytes.Clone();

        byte[] result = HmacKeyNormalizer.NormalizeHmacSha256Key(keyBytes);

        Assert.AreEqual(32, result.Length);
        CollectionAssert.AreEqual(expectedKey, result); // Ensure key is unchanged
    }

    [TestMethod]
    public void Key_Greater_Than_32_Bytes_Should_Remain_Unchanged()
    {
        byte[] keyBytes = new byte[64]; // Larger than 32 bytes
        new Random().NextBytes(keyBytes);
        byte[] expectedKey = (byte[])keyBytes.Clone();

        byte[] result = HmacKeyNormalizer.NormalizeHmacSha256Key(keyBytes);

        Assert.AreEqual(64, result.Length);
        CollectionAssert.AreEqual(expectedKey, result); // Ensure key is unchanged
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Null_Key_Should_Throw_ArgumentNullException()
    {
        HmacKeyNormalizer.NormalizeHmacSha256Key(null!);
    }
}
