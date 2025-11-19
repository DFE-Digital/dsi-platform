using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.Interactions;

[TestClass]
public sealed class DistributedCacheInteractionLimiterExtensionsTests
{
    private sealed record FakeRequest : IKeyedRequest
    {
        public string Key => "FakeKey";
    }

    #region AddInteractionLimiter<TRequest>(IServiceCollection, IConfigurationRoot)

    [TestMethod]
    public void AddInteractionLimiter_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => DistributedCacheInteractionLimiterExtensions.AddInteractionLimiter<FakeRequest>(null!, configuration));
    }

    [TestMethod]
    public void AddInteractionLimiter_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => DistributedCacheInteractionLimiterExtensions.AddInteractionLimiter<FakeRequest>(services, null!));
    }

    [TestMethod]
    public void AddInteractionLimiter_RegistersExpectedServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("InteractionLimiter:FakeRequest:TimePeriodInSeconds", "1234"),
                new("InteractionLimiter:FakeRequest:InteractionsPerTimePeriod", "5"),
            ])
            .Build();

        var services = new ServiceCollection();

        DistributedCacheInteractionLimiterExtensions.AddInteractionLimiter<FakeRequest>(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionLimiter) &&
                descriptor.ImplementationType == typeof(DistributedCacheInteractionLimiter)
            )
        );

        var provider = services.BuildServiceProvider();
        var optionsAccessor = provider.GetRequiredService<IOptionsMonitor<DistributedCacheInteractionLimiterOptions>>();
        var options = optionsAccessor.Get("FakeRequest");
        Assert.AreEqual(1234, options.TimePeriodInSeconds);
        Assert.AreEqual(5, options.InteractionsPerTimePeriod);
    }

    #endregion

    #region Get<TRequest>(IOptionsMonitor<DistributedCacheInteractionLimiterOptions>)

    [TestMethod]
    public void Get_Throws_WhenOptionsAccessorArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => DistributedCacheInteractionLimiterExtensions.Get<FakeRequest>(null!));
    }

    [TestMethod]
    public void Get_AccessesTheExpectedOptionsKey()
    {
        var mockOptionsAccessor = new Mock<IOptionsMonitor<DistributedCacheInteractionLimiterOptions>>();

        DistributedCacheInteractionLimiterExtensions.Get<FakeRequest>(mockOptionsAccessor.Object);

        mockOptionsAccessor.Verify(x => x.Get(
            It.Is<string>(key => key == "FakeRequest")
        ), Times.Once);
    }

    #endregion
}
