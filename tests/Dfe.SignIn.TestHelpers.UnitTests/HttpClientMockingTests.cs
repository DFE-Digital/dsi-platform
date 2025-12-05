using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class HttpClientMockingTests
{
    private sealed record ExampleResponse
    {
        [JsonPropertyName("value")]
        public required int Value { get; init; }
    }

    #region GetHandlerToCaptureRequestUri(Action<Uri>, string, HttpStatusCode)

    [TestMethod]
    public async Task GetHandlerToCaptureRequestUri_ProducesExpectedMock()
    {
        Uri? capturedUri = null;
        var mock = HttpClientMocking.GetHandlerToCaptureRequestUri(uri => capturedUri = uri,
            /*lang=json,strict*/ """
            {
              "value": 123
            }
            """,
            HttpStatusCode.OK
        );

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };
        var responseMessage = await client.PostAsJsonAsync("/test", "someRequestValue");
        var response = await responseMessage.Content.ReadFromJsonAsync<ExampleResponse>();

        Assert.AreEqual(new Uri("https://example.com/test"), capturedUri);
        Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);
        Assert.AreEqual(123, response!.Value);
    }

    #endregion

    #region GetHandlerWithDefaultResponse(HttpStatusCode, string)

    [TestMethod]
    public async Task GetHandlerWithDefaultResponse_ProducesExpectedMock_WhenBodyIsNotNull()
    {
        var mock = HttpClientMocking.GetHandlerWithDefaultResponse(
            HttpStatusCode.OK,
            /*lang=json,strict*/ """{ "value": 123 }"""
        );

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };
        var responseMessage = await client.PostAsJsonAsync("/test", "someRequestValue");
        var response = await responseMessage.Content.ReadFromJsonAsync<ExampleResponse>();

        Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);
        Assert.AreEqual(123, response!.Value);
    }

    [TestMethod]
    public async Task GetHandlerWithDefaultResponse_ProducesExpectedMock_WhenBodyIsNull()
    {
        var mock = HttpClientMocking.GetHandlerWithDefaultResponse(
            HttpStatusCode.NotFound,
            /*lang=json,strict*/ """null"""
        );

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };
        var responseMessage = await client.PostAsJsonAsync("/test", "someRequestValue");
        var response = await responseMessage.Content.ReadFromJsonAsync<ExampleResponse>();

        Assert.AreEqual(HttpStatusCode.NotFound, responseMessage.StatusCode);
        Assert.IsNull(response);
    }

    #endregion

    #region GetHandlerWithMappedResponses(Dictionary<string, MappedResponse>)

    [TestMethod]
    public async Task GetHandlerWithMappedResponses_ProducesExpectedMock_WhenBodyIsNotNull()
    {
        var mock = HttpClientMocking.GetHandlerWithMappedResponses(new() {
            ["(GET) https://example.com/first"] =
                new MappedResponse(HttpStatusCode.NotFound),

            ["(GET) https://example.com/second"] =
                new MappedResponse(HttpStatusCode.OK,
                /*lang=json,strict*/ """
                {
                    "value": 123
                }
                """),

            ["(POST) https://example.com/third"] =
                new MappedResponse(HttpStatusCode.OK,
                /*lang=json,strict*/ """
                {
                    "value": 456
                }
                """),
        });

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };

        var firstResponseMessage = await client.GetAsync("/first");
        Assert.AreEqual(HttpStatusCode.NotFound, firstResponseMessage.StatusCode);
        var firstResponse = await firstResponseMessage.Content.ReadFromJsonAsync<ExampleResponse>();
        Assert.IsNull(firstResponse);

        var secondResponse = await client.GetFromJsonAsync<ExampleResponse>("/second");
        Assert.IsNotNull(secondResponse);
        Assert.AreEqual(123, secondResponse.Value);

        var thirdResponseMessage = await client.PostAsJsonAsync("/third", "someRequestValue");
        Assert.AreEqual(HttpStatusCode.OK, thirdResponseMessage.StatusCode);
        var thirdResponse = await thirdResponseMessage.Content.ReadFromJsonAsync<ExampleResponse>();
        Assert.IsNotNull(thirdResponse);
        Assert.AreEqual(456, thirdResponse.Value);
    }

    [TestMethod]
    public async Task GetHandlerWithMappedResponses_UpdatesInvocations()
    {
        var responseMappings = new Dictionary<string, MappedResponse> {
            ["(GET) https://example.com/first"] =
                new MappedResponse(HttpStatusCode.OK),

            ["(GET) https://example.com/second"] =
                new MappedResponse(HttpStatusCode.OK),

            ["(POST) https://example.com/third"] =
                new MappedResponse(HttpStatusCode.OK),
        };

        var mock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };

        await client.GetAsync("/first");
        await client.PostAsJsonAsync("/third", new { value = 42 });
        await client.PostAsJsonAsync("/third", new { value = 42 });

        Assert.HasCount(1, responseMappings["(GET) https://example.com/first"].Invocations);
        Assert.HasCount(0, responseMappings["(GET) https://example.com/second"].Invocations);

        Assert.HasCount(2, responseMappings["(POST) https://example.com/third"].Invocations);
        var thirdInvocation = responseMappings["(POST) https://example.com/third"].Invocations[0];
        var thirdBody = JsonSerializer.Deserialize<JsonElement>(thirdInvocation.Body!);
        Assert.AreEqual(42, thirdBody.GetProperty("value").GetInt32());
    }

    [TestMethod]
    public async Task GetHandlerWithMappedResponses_Throws_WhenResponseHasNotBeenMapped()
    {
        var responseMappings = new Dictionary<string, MappedResponse> {
            ["(GET) https://example.com/first"] =
                new MappedResponse(HttpStatusCode.OK),
        };

        var mock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);

        var client = new HttpClient(mock.Object) { BaseAddress = new Uri("https://example.com") };

        await Assert.ThrowsExactlyAsync<KeyNotFoundException>(()
            => client.GetAsync("/unmapped"));
    }

    #endregion
}
