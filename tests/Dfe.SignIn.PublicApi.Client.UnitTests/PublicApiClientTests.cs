using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiClientTests
{
    #region Property: HttpClient

    [TestMethod]
    public void HttpClient_HasInitializedValue()
    {
        var mockHttpClient = new Mock<HttpClient>();

        var client = new PublicApiClient(mockHttpClient.Object);

        Assert.AreSame(mockHttpClient.Object, client.HttpClient);
    }

    #endregion
}
