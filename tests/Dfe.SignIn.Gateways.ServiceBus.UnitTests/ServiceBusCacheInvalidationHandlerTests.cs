using Azure.Messaging.ServiceBus;
using Dfe.SignIn.Base.Framework.Caching;
using Moq;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceBusCacheInvalidationHandlerTests
{
    public sealed record ExampleRequest : ICacheableRequest
    {
        public string CacheKey => CacheableRequestConstants.DefaultCacheKey;
    }

    [TestMethod]
    public async Task DoesNotRemoveCacheEntry_WhenCacheKeyIsNull()
    {
        var mockInteractionCache = new Mock<IInteractionCache<ExampleRequest>>();
        var handler = new ServiceBusCacheInvalidationHandler<ExampleRequest>(
            mockInteractionCache.Object,
            _ => null
        );

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage();

        await handler.HandleAsync(fakeMessage, CancellationToken.None);

        mockInteractionCache.Verify(x =>
            x.RemoveAsync(
                It.IsAny<string>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task RemovesCacheWithResolvedKey()
    {
        var mockInteractionCache = new Mock<IInteractionCache<ExampleRequest>>();
        var handler = new ServiceBusCacheInvalidationHandler<ExampleRequest>(
            mockInteractionCache.Object,
            message => message.Subject
        );

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(subject: "example");

        await handler.HandleAsync(fakeMessage, CancellationToken.None);

        mockInteractionCache.Verify(x =>
            x.RemoveAsync(
                It.Is<string>(p => Equals("example", p))
            ),
            Times.Once
        );
    }
}
