using Dfe.SignIn.PublicApi.Client.Abstractions;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.Abstractions;

[TestClass]
public sealed class HttpRequestExtensionsTests
{
    #region GetRequiredQuery(IHttpRequest, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetRequiredQuery_Throws_WhenRequestArgumentIsNull()
    {
        HttpRequestExtensions.GetRequiredQuery(null!, "abc");
    }

    [TestMethod]
    public void GetRequiredQuery_Throws_WhenParameterIsMissing()
    {
        var mockRequest = new Mock<IHttpRequest>();

        mockRequest.Setup(mock =>
            mock.GetQuery(
                It.Is<string>(key => key == "abc")
            ))
            .Returns((string)null!);

        var exception = Assert.Throws<KeyNotFoundException>(
            () => HttpRequestExtensions.GetRequiredQuery(mockRequest.Object, "abc")
        );

        Assert.AreEqual("Missing required parameter 'abc'.", exception.Message);
    }

    #endregion
}
