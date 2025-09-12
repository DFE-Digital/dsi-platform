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
    public static Mock<HttpMessageHandler> GetHandlerToCaptureRequestUri(
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

    /// <summary>
    /// Creates a mock message handler for use with a <see cref="HttpClient"/> instance
    /// which uses the mocked response for any requests.
    /// </summary>
    /// <param name="status">The mocked HTTP status code.</param>
    /// <param name="mockResponseJson">The mock response with JSON encoding.</param>
    /// <returns>
    ///   <para>The mock <see cref="HttpMessageHandler"/> instance.</para>
    /// </returns>
    public static Mock<HttpMessageHandler> GetHandlerWithDefaultResponse(
        HttpStatusCode status = HttpStatusCode.OK,
        string mockResponseJson = "null")
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _)
                => new HttpResponseMessage {
                    StatusCode = status,
                    Content = new StringContent(mockResponseJson, Encoding.UTF8, "application/json"),
                });
        return mock;
    }

    /// <summary>
    /// Creates a mock message handler for use with a <see cref="HttpClient"/> instance.
    /// </summary>
    /// <remarks>
    ///   <para>Keys of request map should be formatted in the following way:</para>
    ///   <code><![CDATA[
    ///     (<method>) <url>
    ///   ]]></code>
    ///   <example>
    ///     <para>An example request mapping key:</para>
    ///     <code><![CDATA[
    ///       (POST) https://example.com/foo
    ///     ]]></code>
    ///   </example>
    /// </remarks>
    /// <param name="responseMap">The mock response mappings.</param>
    /// <returns>
    ///   <para>The mock <see cref="HttpMessageHandler"/> instance.</para>
    /// </returns>
    public static Mock<HttpMessageHandler> GetHandlerWithMappedResponses(
        Dictionary<string, MappedResponse> responseMap)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _)
                => {
                    string key = $"({req.Method}) {req.RequestUri}";
                    if (responseMap.TryGetValue(key, out var mapping)) {
                        ++mapping.InvocationCount;
                        return new HttpResponseMessage {
                            StatusCode = mapping.Status,
                            Content = new StringContent(mapping.Body, Encoding.UTF8, "application/json"),
                        };
                    }
                    else {
                        throw new KeyNotFoundException($"Request has not been mapped '{key}'!");
                    }
                });
        return mock;
    }
}

/// <summary>
/// Represents a mapped response.
/// </summary>
/// <param name="status">The HTTP status code.</param>
/// <param name="body">The JSON encoded response body.</param>
public sealed class MappedResponse(
    HttpStatusCode status = HttpStatusCode.OK, string body = "null")
{
    /// <summary>
    /// Gets the HTTP status code; defaults to a value of <see cref="HttpStatusCode.OK"/>.
    /// </summary>
    public HttpStatusCode Status => status;

    /// <summary>
    /// Gets the JSON encoded response body; defaults to a value of "null".
    /// </summary>
    public string Body => body;

    /// <summary>
    /// Gets or sets the number of times the request was invoked.
    /// </summary>
    public int InvocationCount { get; set; } = 0;
}
