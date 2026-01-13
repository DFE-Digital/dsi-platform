
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class ResilientServiceCollectionExtensionsTests
{
    #region SetupResilientHttpClient(IServiceCollection, IEnumerable<string>, IConfigurationRoot, string)

    [TestMethod]
    public void SetupResilientHttpClient_Throws_WhenServicesIsNull()
    {
        var configuration = BuildValidConfiguration();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            ResilientServiceCollectionExtensions.SetupResilientHttpClient(
                services: null!,
                httpClientNames: ["ApiDirectories"],
                configurationRoot: configuration,
                defaultStrategyName: "Default"));
    }

    [TestMethod]
    public void SetupResilientHttpClient_Throws_WhenConfigurationRootIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            ResilientServiceCollectionExtensions.SetupResilientHttpClient(
                services,
                httpClientNames: ["ApiDirectories"],
                configurationRoot: null!,
                defaultStrategyName: "Default"));
    }

    [TestMethod]
    public void SetupResilientHttpClient_Throws_WhenDefaultStrategyNameIsNullOrWhitespace()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        Assert.ThrowsExactly<ArgumentException>(() =>
            services.SetupResilientHttpClient(
                httpClientNames: ["ApiDirectories"],
                configurationRoot: configuration,
                defaultStrategyName: " "));
    }

    [TestMethod]
    public void SetupResilientHttpClient_Throws_WhenHttpResiliencySectionIsMissing()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.SetupResilientHttpClient(
                httpClientNames: ["ApiDirectories"],
                configurationRoot: configuration,
                defaultStrategyName: "Default"));
    }

    [TestMethod]
    public void SetupResilientHttpClient_RegistersResilientHttpMessageHandler()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        services.SetupResilientHttpClient(
            httpClientNames: ["ApiDirectories"],
            configurationRoot: configuration,
            defaultStrategyName: "Default");

        var provider = services.BuildServiceProvider();

        var handler = provider.GetService<ResilientHttpMessageHandler>();

        Assert.IsNotNull(handler);
    }

    [TestMethod]
    public void SetupResilientHttpClient_RegistersNamedHttpClients()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        services.SetupResilientHttpClient(
            httpClientNames: ["ApiDirectories", "ApiServices"],
            configurationRoot: configuration,
            defaultStrategyName: "Default");

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();

        Assert.IsNotNull(factory.CreateClient("ApiDirectories"));
        Assert.IsNotNull(factory.CreateClient("ApiServices"));
    }

    [TestMethod]
    public void SetupResilientHttpClient_RegistersResiliencePipelines()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        services.SetupResilientHttpClient(
            httpClientNames: ["ApiDirectories"],
            configurationRoot: configuration,
            defaultStrategyName: "Default");

        var provider = services.BuildServiceProvider();
        var pipelineProvider = provider.GetRequiredService<ResiliencePipelineProvider<string>>();

        Assert.IsNotNull(
            pipelineProvider.GetPipeline<HttpResponseMessage>("InternalApiDefault"));
    }

    [TestMethod]
    public void SetupResilientHttpClient_RegistersHandlerOptions_WithDefaultStrategy()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        services.SetupResilientHttpClient(
            httpClientNames: ["ApiDirectories"],
            configurationRoot: configuration,
            defaultStrategyName: "InternalApiDefault");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ResilientHttpMessageHandlerOptions>();

        Assert.AreEqual("InternalApiDefault", options.DefaultStrategyName);
    }

    [TestMethod]
    public void SetupResilientHttpClient_ReturnsServices_ForChaining()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        var result = services.SetupResilientHttpClient(
            httpClientNames: ["ApiDirectories"],
            configurationRoot: configuration,
            defaultStrategyName: "Default");

        Assert.AreSame(services, result);
    }

    #endregion

    #region Helpers

    private static IConfigurationRoot BuildValidConfiguration()
    {
        var data = new Dictionary<string, string?> {
            ["HttpResiliency:InternalApiDefault:Timeout"] = "5",
            ["HttpResiliency:InternalApiDefault:Retry:MaxRetryAttempts"] = "3",
            ["HttpResiliency:InternalApiDefault:Retry:Delay"] = "200",
            ["HttpResiliency:InternalApiDefault:Retry:UseJitter"] = "true",
            ["HttpResiliency:InternalApiDefault:Retry:BackoffType"] = "Exponential"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    #endregion
}
