using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class ResilientServiceCollectionExtensionsTests
{
    #region SetupResiliencePipelines(IServiceCollection, IConfigurationRoot)

    [TestMethod]
    public void SetupResiliencePipelines_Throws_WhenServicesIsNull()
    {
        var configuration = BuildValidConfiguration();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            ResilientServiceCollectionExtensions.SetupResiliencePipelines(
                services: null!, configurationRoot: configuration));
    }

    [TestMethod]
    public void SetupResiliencePipelines_Throws_WhenConfigurationRootIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            ResilientServiceCollectionExtensions.SetupResiliencePipelines(
                services, configurationRoot: null!));
    }

    [TestMethod]
    public void SetupResiliencePipelines_Throws_WhenHttpResiliencySectionIsMissing()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            ResilientServiceCollectionExtensions.SetupResiliencePipelines(services, configuration));
    }

    [TestMethod]
    public void SetupResiliencePipelines_RegistersResiliencePipelines()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        ResilientServiceCollectionExtensions.SetupResiliencePipelines(services, configuration);

        var provider = services.BuildServiceProvider();
        var pipelineProvider = provider.GetRequiredService<ResiliencePipelineProvider<string>>();

        Assert.IsNotNull(
            pipelineProvider.GetPipeline<HttpResponseMessage>("InternalApiDefault")
        );
    }

    [TestMethod]
    public void SetupResiliencePipelines_ReturnsServices_ForChaining()
    {
        var services = new ServiceCollection();
        var configuration = BuildValidConfiguration();

        var result = ResilientServiceCollectionExtensions.SetupResiliencePipelines(services, configuration);

        Assert.AreSame(services, result);
    }

    #endregion

    #region Helpers

    private static IConfigurationRoot BuildValidConfiguration()
    {
        var data = new Dictionary<string, string?>([
            new("HttpResiliency:InternalApiDefault:Timeout", "5"),
            new("HttpResiliency:InternalApiDefault:Retry:MaxRetryAttempts", "3"),
            new("HttpResiliency:InternalApiDefault:Retry:Delay", "200"),
            new("HttpResiliency:InternalApiDefault:Retry:UseJitter", "true"),
            new("HttpResiliency:InternalApiDefault:Retry:BackoffType", "Exponential"),
        ]);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();
    }

    #endregion
}
