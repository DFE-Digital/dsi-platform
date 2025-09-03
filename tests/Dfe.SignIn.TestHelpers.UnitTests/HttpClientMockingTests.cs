using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class HttpClientMockingTests
{
    #region CreateMockMessageHandlerWithJson(Action<Uri>, string, HttpStatusCode)

    private sealed record ExampleResponse
    {
        [JsonPropertyName("value")]
        public required int Value { get; init; }
    }

    [TestMethod]
    public async Task CreateMockMessageHandlerWithJson()
    {
        Uri? capturedUri = null;
        var mock = HttpClientMocking.CreateMockMessageHandlerWithJson(uri => capturedUri = uri,
            /*lang=json,strict*/ """
            {
              "value": 123
            }
            """,
            HttpStatusCode.OK
        );

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("/") };
        var responseMessage = await client.PostAsJsonAsync("/test", "someValue");
        var response = await responseMessage.Content.ReadFromJsonAsync<ExampleResponse>();

        Assert.AreEqual(new Uri("/test"), capturedUri);
        Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);
        Assert.AreEqual(123, response!.Value);
    }

    #endregion
}
