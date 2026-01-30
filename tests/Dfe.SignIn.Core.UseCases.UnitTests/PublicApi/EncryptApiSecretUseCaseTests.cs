using System.Text;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

[TestClass]
public sealed class EncryptApiSecretUseCaseTests
{
    private static readonly string EncryptionKey = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(new string('F', 32)));

    [TestMethod]
    public async Task EncryptsApiSecret_ReturnsExpectedValuePrefix()
    {
        var encryptUseCase = PublicApiHelpers.CreateEncryptionUseCase(EncryptionKey);
        var context = PublicApiHelpers.CreateEncryptionInteractionContext("super-secret");
        var response = await encryptUseCase.InvokeAsync(context, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsFalse(string.IsNullOrWhiteSpace(response.EncryptedApiSecret));
        Assert.StartsWith("ENC:0:", response.EncryptedApiSecret);
    }

    [TestMethod]
    public async Task EncryptedValue_CanBeDecryptedBackToOriginal()
    {
        var encryptUseCase = PublicApiHelpers.CreateEncryptionUseCase(EncryptionKey);
        var encryptedResponse = await encryptUseCase.InvokeAsync(
            PublicApiHelpers.CreateEncryptionInteractionContext("super-secret")
        );

        var decryptUseCase = PublicApiHelpers.CreateDecryptionUseCase(EncryptionKey);
        var decryptContext = PublicApiHelpers.CreateDecryptInteractionContext(encryptedResponse.EncryptedApiSecret);
        var result = await decryptUseCase.InvokeAsync(decryptContext);

        Assert.AreEqual("super-secret", result.ApiSecret);
    }
}
