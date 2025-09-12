using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Helper functionality for mocking requests from the perspective of the server.
/// </summary>
public static class HttpServerMocking
{
    /// <summary>
    /// Create a fake HTTP request with a JSON encoded body.
    /// </summary>
    /// <typeparam name="TBody">The type of body.</typeparam>
    /// <param name="body">The body data.</param>
    /// <returns>
    ///   <para>The HTTP request populated with the JSON encoded content.</para>
    /// </returns>
    public static HttpRequest CreateJsonRequest<TBody>(TBody body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        string json = JsonSerializer.Serialize(body);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        request.Body = new MemoryStream(jsonBytes);
        request.ContentType = "application/json";

        return request;
    }
}
