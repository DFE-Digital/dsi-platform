using Dfe.SignIn.Base.Framework.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Base.Framework.UnitTests.Caching;

[TestClass]
public sealed class InMemoryInteractionCacheExtensionsTests
{
    private sealed record ExampleRequest : IKeyedRequest
    {
        public string Key => "abc";
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

    #region AddInMemoryInteractionCache<TRequest>

    [TestMethod]
    public void AddInMemoryInteractionCache_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InMemoryInteractionCacheExtensions.AddInMemoryInteractionCache<ExampleRequest>(
                services: null!,
                configureOptions: null
            ));
    }

    [TestMethod]
    public void AddInMemoryInteractionCache_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        var result = InMemoryInteractionCacheExtensions.AddInMemoryInteractionCache<ExampleRequest>(services);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddInMemoryInteractionCache_SetsUpInteractionCacheForRequestType()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        InMemoryInteractionCacheExtensions.AddInMemoryInteractionCache<ExampleRequest>(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionCache<ExampleRequest>) &&
                descriptor.ImplementationFactory is not null
            )
        );
    }

    [TestMethod]
    public void AddInMemoryInteractionCache_DecoratesInteractorWithCachedInteractor()
    {
        var services = new ServiceCollection();

        services.AddInteractor<FakeInteractor>();

        InMemoryInteractionCacheExtensions.AddInMemoryInteractionCache<ExampleRequest>(services, options => {
            options.DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        });

        var provider = services.BuildServiceProvider();
        var interactor = provider.GetRequiredService<IInteractor<ExampleRequest>>();

        Assert.IsInstanceOfType<CachedInteractor<ExampleRequest>>(interactor);
    }

    #endregion
}
