using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Helper functionality for mocking HttpClient requests.
/// </summary>
public static class HttpClientMocking
{
    /// <summary>
    /// Creates a mock message handler for use with a <see cref="HttpClient"/> instance.
    /// </summary>
    /// <param name="captureRequestUri">Delegate to capture the requested URI.</param>
    /// <param name="mockResponseJson">The mock response with JSON encoding.</param>
    /// <param name="statusCode">The mocked HTTP status code.</param>
    /// <returns>
    ///   <para>The mock <see cref="HttpMessageHandler"/> instance.</para>
    /// </returns>
    public static Mock<HttpMessageHandler> CreateMockMessageHandlerWithJson(
        Action<Uri> captureRequestUri,
        string mockResponseJson = "null",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _)
                => captureRequestUri(req.RequestUri!)
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
                Content = new StringContent(mockResponseJson, Encoding.UTF8, "application/json"),
            });
        return mock;
    }
}
