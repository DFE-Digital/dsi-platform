using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dfe.SignIn.Core.UseCases.PublicApiSigning;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.Tests.PublicApiSigning;

[TestClass]
public sealed class CreateDigitalSignatureForPayload_UseCaseTests
{
    // Note: The following private key in these unit tests are not being used
    // by any systems and were generated exclusively for testing usage.
    private const string FakePrivateKey = @"-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCf0eB3c6ZPSG4JvlvIPh4WCMZN
rpls62al9c8sCE/tlOE0hoTa+x7IUvCftBT+XuV6hSDji6eR/SoCjpC918YdSu6fT7q/F7MZzAqX
Y+FFjYNozCtMAEsON8X7D/J56j5ZIBIxEYr46fs7Sq3QT9AqUuQ8C5UqxE4dMjrMnwdN0Nq3/JrY
UniFnWvcsdCJmYXt2cOp140TPVx28fdRUv+vjpjt/v42KSPy/Hnw9OLXuEcyWsL3BRc3HfsYPyQg
FPQjol/9g2bmZcT3rmyH9vZBB8YkH65FiQ0WmXrgW2ub5Fg80hvXFvd9+zai3Sln1FYdLEBhAczZ
NiobTP93OujlAgMBAAECggEAJj1wiQRZ+cRp19j9WwdJ6ZnF4RZyzXXxxKnxHScMANvLmubI6SCG
+AWoX34WO9r2637pJKjoumyp3ZzBEzuKwr2IJQRNuaVxDC4fJqQWZa77j1qyzWeQjeFYdL8XFIaw
zmB3GdiJuQ87Nq+isSI3u7jDtX5L/cSksCWxnf47ICoEn+++PG8o0YEIyIm1pfXp48GDlM3cood+
1DyJ5Jo/vYSVvEva0xbLGQq2ekNJENWOVYCM257mD9C9eXL1deRfDwK/UaKmj3yYGHhY9PruBDtN
g93ojced+sg9RgLf1DqO+CzjPj6FEClenNLRJdHXg6lw6OHVZ+gjGLuEDj9BAQKBgQDhPzRa5lKW
roh7IBQpuYxdt3EQNcPH57oYpYzOBVjKC9KpXvBOwdyz6DonIBs/Eurjx6pNjJDzuyIpIVMaQYuu
3THBMeBu7XOv8NVTOF6bQ2dGC8/ni+gXcWMccqfeIBsGfjX31HXJ1yy9uO8XCBp4NMUdfUzCFsjP
jIBvtXQxpQKBgQC1o+Ahk7agJuIZHeAC/eaIjf0YHHeooGreaIv/XuFGYIRLW+ZkX9D7NARFHn+A
Vyqe1zC9DJ5k57wx7DSXKzhDq0TxuQJ8YuET6mKu4TQ7M85YsuQJmXLLhOJjvx067IHw5/b4Bvne
pDQmsgpyKViT6Zhp4j35vHnPXmqpOoO2QQKBgEu7p4vEkxIsvqC/SWg6BbLg4bf0i84j4JM23l/K
tm7AiOT/kca5Mc6fjyXmiqKrZqSNVnpaf8YjLjosBTf3v9JcdsUhUveZCOOoEuG3Oz/y6r9Ha3DL
vo5bRlqjRkPOAaguOVEJc00Y9ucXTfQtelDeVUQ0A+HiLURzVh+5H8ctAoGAH233vlec5iFURhxR
QrNETKSlAqMKYXdAhrN4Zfu450CUI6YHO58Ivi7F/l/EmCR9D3cUy+F/Ft6yRcElaHLmDW95QopM
z9EcOSH5aWE2dHgGYHqz0qVmo6ies7vCBwwdf93jcg9i9Q9cpsVv4UkeNpnY8ZlgN/JQ8XzY3+ds
GwECgYEAyDZqKrvUXEfMKx2Q7gpfGrzFOlj/eM5uZ2uvbGnG1MaodddUrs1vZKVY6OsI63Mu37VJ
WZ7f+6bH5HAERcEj945GnRskE9MgETd6r+SDCSDnUlBipdx5Co0gcfKUTWWrU4DUUgieifCnFgc0
1SmMPtMBadiIC72shgNBwocJ8aQ=
-----END PRIVATE KEY-----";

    // Note: The following public key in these unit tests are not being used
    // by any systems and were generated exclusively for testing usage.
    private const string FakePublicKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn9Hgd3OmT0huCb5byD4eFgjGTa6ZbOtm
pfXPLAhP7ZThNIaE2vseyFLwn7QU/l7leoUg44unkf0qAo6QvdfGHUrun0+6vxezGcwKl2PhRY2D
aMwrTABLDjfF+w/yeeo+WSASMRGK+On7O0qt0E/QKlLkPAuVKsROHTI6zJ8HTdDat/ya2FJ4hZ1r
3LHQiZmF7dnDqdeNEz1cdvH3UVL/r46Y7f7+Nikj8vx58PTi17hHMlrC9wUXNx37GD8kIBT0I6Jf
/YNm5mXE965sh/b2QQfGJB+uRYkNFpl64Ftrm+RYPNIb1xb3ffs2ot0pZ9RWHSxAYQHM2TYqG0z/
dzro5QIDAQAB
-----END PUBLIC KEY-----";

    #region InvokeAsync(CreateDigitalSignatureForPayloadRequest)

    private static CreateDigitalSignatureForPayload_UseCase CreateMockedUseCase(
        AutoMocker? autoMocker = null)
    {
        autoMocker ??= new AutoMocker();

        autoMocker.GetMock<IOptions<PublicApiSigningOptions>>()
            .Setup(x => x.Value)
            .Returns(new PublicApiSigningOptions {
                Algorithm = HashAlgorithmName.MD5,
                Padding = RSASignaturePadding.Pkcs1,
                PrivateKeyPem = FakePrivateKey,
                PublicKeyId = "ExampleKey1",
            });

        return autoMocker.CreateInstance<CreateDigitalSignatureForPayload_UseCase>();
    }

    [TestMethod]
    public async Task InvokeAsync_CreatesExpectedDigitalSignatureForPayload()
    {
        var useCase = CreateMockedUseCase();

        var response = await useCase.InvokeAsync(new() {
            Payload = "Hello, world!",
        });

        string expectedDigitalSignature = Regex.Replace(
            @"Ginm9woUBpxxWyzOHIUHNlb4aDbjcycPA5R/tMHeBJh3pCSSFch7KXP5oCee3MuJDo
            ZoB9wIBJcGEAYkM/kiooY64fC8YkOPlWYpfLWaDtLZqaGVzfVH4tmst73vn51O9haMq9
            ZBkWQM/VMaFV5W0yGgEq2rt7W1kQE1jdmUQXK+SqauzLP/0gggIPtGUMG7XcjFDf5f6A
            L0PVN8DUiROOQt3+CgDBsZlW1TeDFUiXRb8K6thck50wp2jgDeqvtfrQogAO6na3QDxU
            uxhCmPdWH7EkOIzX4Zj1pGJyFl+IiydWdmd3vfQH9yJFXLonq22DeHXw9xW9aScDEYsf
            cm9A==", @"\s+", "", RegexOptions.Multiline
        );
        Assert.AreEqual(expectedDigitalSignature, response.Signature.Signature);
        Assert.AreEqual("ExampleKey1", response.Signature.KeyId);
    }

    [TestMethod]
    public async Task InvokeAsync_DigitalSignatureCanBeVerified()
    {
        var useCase = CreateMockedUseCase();

        string payload = "Hello, world!";
        var response = await useCase.InvokeAsync(new() {
            Payload = payload,
        });

        using var key = RSA.Create();
        key.ImportFromPem(FakePublicKey);

        Assert.IsTrue(key.VerifyData(
            Encoding.UTF8.GetBytes(payload),
            Convert.FromBase64String(response.Signature.Signature),
            HashAlgorithmName.MD5,
            RSASignaturePadding.Pkcs1
        ));
    }

    #endregion
}
