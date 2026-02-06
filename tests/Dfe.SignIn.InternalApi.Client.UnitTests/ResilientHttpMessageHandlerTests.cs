using System.Net;
using Dfe.SignIn.InternalApi.Client.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Timeout;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class ResilientHttpMessageHandlerTests
{
    private static HttpClient SetupHttpClient(HttpMessageHandler innerHandler)
    {
        var services = new ServiceCollection();

        services.AddResiliencePipeline<string, HttpResponseMessage>("slow", builder => { });

        services.AddResiliencePipeline<string, HttpResponseMessage>("fast", builder => { });

        services.AddResiliencePipeline<string, HttpResponseMessage>("default", builder => {
            builder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage> {
                MaxRetryAttempts = 3,
                UseJitter = false,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(r => (int)r.StatusCode >= 500)
            });
        });

        services.AddSingleton(innerHandler);
        services.AddTransient(provider =>
            ActivatorUtilities.CreateInstance<ResilientHttpMessageHandler>(provider, "default"));

        var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<ResilientHttpMessageHandler>();
        handler.InnerHandler = innerHandler;

        return new HttpClient(handler);
    }

    [TestMethod]
    [DataRow(null, "default")]
    [DataRow("slow", "slow")]
    [DataRow("fast", "fast")]
    [DataRow("doesNotExist", "default")]
    public async Task SendAsync_UsesCorrectPipeline(string requestedPipeline, string expectedPipeline)
    {
        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return Task.FromResult(response);
        });

        var client = SetupHttpClient(fakeHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");
        if (requestedPipeline is not null) {
            request.Options.Set(ResilientHttpMessageHandlerOptions.RequestedResiliencePipeline, requestedPipeline);
        }

        await client.SendAsync(request);

        request.Options.TryGetValue(ResilientHttpMessageHandlerOptions.UsedResiliencePipeline, out var actualStrategyUsed);
        Assert.IsNotNull(actualStrategyUsed);
        Assert.AreEqual(expectedPipeline, actualStrategyUsed);
    }

    [TestMethod]
    public async Task SendAsync_RetriesOn500StatusCode()
    {
        int invocationCount = 0;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            invocationCount++;
            if (invocationCount < 2) {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var client = SetupHttpClient(fakeHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");

        var response = await client.SendAsync(request);

        Assert.AreEqual(2, invocationCount);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task SendAsync_RetriesOnHttpRequestException()
    {
        int attempts = 0;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            attempts++;
            if (attempts < 2) {
                throw new HttpRequestException("Simulated network error");
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var client = SetupHttpClient(fakeHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");
        var response = await client.SendAsync(request);

        Assert.AreEqual(2, attempts);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task SendAsync_RetriesOnTimeoutRejectedException()
    {
        int attempts = 0;

        var fakeHandler = new FakeHttpMessageHandler((req, ct) => {
            attempts++;
            if (attempts < 2) {
                throw new TimeoutRejectedException("Simulated timeout");
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var client = SetupHttpClient(fakeHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://mock.localhost");
        var response = await client.SendAsync(request);

        Assert.AreEqual(2, attempts);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
