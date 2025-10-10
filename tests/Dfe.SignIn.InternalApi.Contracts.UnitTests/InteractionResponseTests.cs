using System.Text.Json;

namespace Dfe.SignIn.InternalApi.Contracts.UnitTests;

[TestClass]
public sealed class InteractionResponseTests
{
    #region Success

    [TestMethod]
    public void Success_IsTrue_WhenExceptionIsNull()
    {
        var interactionResponse = new InteractionResponse {
            Exception = null,
        };

        Assert.IsTrue(interactionResponse.Success);
    }

    [TestMethod]
    public void Success_IsFalse_WhenExceptionIsNotNull()
    {
        var interactionResponse = new InteractionResponse {
            Exception = JsonDocument.Parse("{}").RootElement,
        };

        Assert.IsFalse(interactionResponse.Success);
    }

    #endregion
}
