
using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.InternalApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class ResilientHttpClientExtensionsTests
{
    #region SendAsync(HttpClient, HttpMethod, IConfigurationRoot, string, TRequest, CancellationToken)

    [TestMethod]
    public async Task SendAsJsonAsync_WithContent()
    {
        HttpRequestMessage? capturedRequest = null;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            capturedRequest = req;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var client = new HttpClient(new ResilientHttpMessageHandlerMock(fakeHandler));

        var payload = new { Name = "Test" };

        await client.SendAsJsonAsync(HttpMethod.Post, "http://mock.localhost/", payload);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Post, capturedRequest.Method);
        Assert.AreEqual("http://mock.localhost/", capturedRequest.RequestUri!.ToString());

        var expectedContent = JsonContent.Create(payload);
        var expectedJson = await expectedContent.ReadAsStringAsync();

        var actualJson = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    public async Task SendAsJsonAsync_WithNullContent()
    {
        HttpRequestMessage? capturedRequest = null;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            capturedRequest = req;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var client = new HttpClient(new ResilientHttpMessageHandlerMock(fakeHandler));

        await client.SendAsJsonAsync(HttpMethod.Get, "http://mock.localhost", (object?)null);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(HttpMethod.Get, capturedRequest.Method);
        Assert.IsNull(capturedRequest.Content);
    }

    [TestMethod]
    public async Task SendAsJsonAsync_WithNamedResiliencePipeline()
    {
        HttpRequestMessage? capturedRequest = null;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            capturedRequest = req;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var client = new HttpClient(new ResilientHttpMessageHandlerMock(fakeHandler));
        var payload = new TestRequest { Value = 42 };
        await client.SendAsJsonAsync(HttpMethod.Put, "http://mock.localhost", payload);

        Assert.IsNotNull(capturedRequest);

        capturedRequest.Options.TryGetValue(ResilientHttpMessageHandlerOptions.RequestedResiliencePipeline, out var usedPipeline);
        Assert.AreEqual(nameof(TestRequest), usedPipeline);
    }

    #endregion

    #region Helpers

    private sealed class TestRequest
    {
        public int Value { get; set; }
    }

    private sealed class ResilientHttpMessageHandlerMock : DelegatingHandler
    {
        public ResilientHttpMessageHandlerMock(HttpMessageHandler innerHandler)
        {
            this.InnerHandler = innerHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    #endregion
}
