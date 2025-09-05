using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiPostRequesterTests
{
    private sealed record ExampleRequest
    {
    }

    private sealed record ExampleResponse
    {
        [JsonPropertyName("value")]
        public required int Value { get; init; }
    }

    private static Mock<IPublicApiClient> CreateMockClient(Mock<HttpMessageHandler> mockMessageHandler)
    {
        var mockClient = new Mock<IPublicApiClient>();
        mockClient
            .Setup(x => x.HttpClient)
            .Returns(new HttpClient(mockMessageHandler.Object) {
                BaseAddress = new Uri("/"),
            });
        return mockClient;
    }

    #region InvokeAsync()

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenThereAreValidationErrors()
    {
        Uri? capturedUri = null;
        var handlerMock = HttpClientMocking.CreateMockMessageHandlerWithJson(
            uri => capturedUri = uri, "{}", HttpStatusCode.BadRequest);

        var mockClient = CreateMockClient(handlerMock);

        var interactionContext = new InteractionContext<ExampleRequest>(new ExampleRequest());
        interactionContext.AddValidationError("Some validation error");

        var instance = new PublicApiPostRequester<ExampleRequest, ExampleResponse>(
            mockClient.Object,
            JsonSerializerOptions.Web,
            "some/endpoint"
        );

        await Assert.ThrowsAsync<InvalidRequestException>(()
            => instance.InvokeAsync(interactionContext));
    }

    [TestMethod]
    public async Task InvokeAsync_InvokesExpectedEndpoint()
    {
        Uri? capturedUri = null;
        var handlerMock = HttpClientMocking.CreateMockMessageHandlerWithJson(uri => capturedUri = uri,
            /*lang=json,strict*/ """
            {
              "value": 123
            }
            """
        );

        var mockClient = CreateMockClient(handlerMock);

        var instance = new PublicApiPostRequester<ExampleRequest, ExampleResponse>(
            mockClient.Object,
            JsonSerializerOptions.Web,
            "some/endpoint"
        );

        var response = await instance.InvokeAsync(new ExampleRequest());

        var expectedUrl = new Uri("/some/endpoint");
        Assert.AreEqual(expectedUrl, capturedUri);
        Assert.IsNotNull(response);
        Assert.AreEqual(123, response.Value);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenResponseIsNotSuccess()
    {
        Uri? capturedUri = null;
        var handlerMock = HttpClientMocking.CreateMockMessageHandlerWithJson(
            uri => capturedUri = uri, "{}", HttpStatusCode.BadRequest);

        var mockClient = CreateMockClient(handlerMock);

        var instance = new PublicApiPostRequester<ExampleRequest, ExampleResponse>(
            mockClient.Object,
            JsonSerializerOptions.Web,
            "some/endpoint"
        );

        await Assert.ThrowsAsync<HttpRequestException>(()
            => instance.InvokeAsync(new ExampleRequest()));
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenResponseDataIsNull()
    {
        Uri? capturedUri = null;
        var handlerMock = HttpClientMocking.CreateMockMessageHandlerWithJson(
            uri => capturedUri = uri, "null");

        var mockClient = CreateMockClient(handlerMock);

        var instance = new PublicApiPostRequester<ExampleRequest, ExampleResponse>(
            mockClient.Object,
            JsonSerializerOptions.Web,
            "some/endpoint"
        );

        await Assert.ThrowsAsync<MissingResponseDataException>(()
            => instance.InvokeAsync(new ExampleRequest()));
    }

    #endregion
}
