using System.Text;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

[TestClass]
public sealed class DecryptApiSecretUseCaseTests
{
    private static readonly string EncryptionKey = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(new string('F', 32)));

    [TestMethod]
    public async Task WhenValueIsNotEncrypted_ReturnsValueUnchanged()
    {
        var decryptUseCase = PublicApiHelpers.CreateDecryptionUseCase(EncryptionKey);
        var context = PublicApiHelpers.CreateDecryptInteractionContext("not-encrypted");
        var result = await decryptUseCase.InvokeAsync(context);

        Assert.AreEqual("not-encrypted", result.ApiSecret);
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
        var decryptResult = await decryptUseCase.InvokeAsync(decryptContext);

        Assert.AreEqual("super-secret", decryptResult.ApiSecret);
    }
}
