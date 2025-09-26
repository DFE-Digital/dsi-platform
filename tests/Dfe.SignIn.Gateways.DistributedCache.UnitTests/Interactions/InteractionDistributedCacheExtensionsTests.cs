using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Caching;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.Interactions;

[TestClass]
public sealed class InteractionDistributedCacheExtensionsTests
{
    private sealed record ExampleRequest : ICacheableRequest
    {
        public string CacheKey => "abc";
    }

    private sealed record ExampleResponse
    {
    }

    private sealed class FakeInteractor : Interactor<ExampleRequest, ExampleResponse>
    {
        public override Task<ExampleResponse> InvokeAsync(
            InteractionContext<ExampleRequest> context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ExampleResponse());
        }
    }

    #region AddDistributedInteractionCache<TRequest, TResponse>

    [TestMethod]
    public void AddDistributedInteractionCache_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionDistributedCacheExtensions.AddDistributedInteractionCache<ExampleRequest, ExampleResponse>(
                services: null!,
                configureOptions: null
            ));
    }

    [TestMethod]
    public void AddDistributedInteractionCache_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        var result = InteractionDistributedCacheExtensions.AddDistributedInteractionCache<ExampleRequest, ExampleResponse>(services);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddDistributedInteractionCache_SetsUpSerializationForCacheEntries()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        InteractionDistributedCacheExtensions.AddDistributedInteractionCache<ExampleRequest, ExampleResponse>(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ICacheEntrySerializer) &&
                descriptor.ImplementationType == typeof(DefaultCacheEntrySerializer)
            )
        );
    }

    [TestMethod]
    public void AddDistributedInteractionCache_SetsUpInteractionCacheForRequestType()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        InteractionDistributedCacheExtensions.AddDistributedInteractionCache<ExampleRequest, ExampleResponse>(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionCache<ExampleRequest>) &&
                descriptor.ImplementationType == typeof(InteractionDistributedCache<ExampleRequest, ExampleResponse>)
            )
        );
    }

    [TestMethod]
    public void AddDistributedInteractionCache_DecoratesInteractorWithCachedInteractor()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        var mockDistributedCache = new Mock<IDistributedCache>();
        services.AddKeyedSingleton(DistributedCacheKeys.InteractionRequests, mockDistributedCache.Object);

        InteractionDistributedCacheExtensions.AddDistributedInteractionCache<ExampleRequest, ExampleResponse>(services, options => {
            options.DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        });

        var provider = services.BuildServiceProvider();
        var interactor = provider.GetRequiredService<IInteractor<ExampleRequest>>();

        Assert.IsInstanceOfType<CachedInteractor<ExampleRequest>>(interactor);
    }

    #endregion
}
