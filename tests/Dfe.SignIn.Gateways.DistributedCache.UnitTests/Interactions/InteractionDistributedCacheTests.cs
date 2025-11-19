using System.Text;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.Interactions;

[TestClass]
public sealed class InteractionDistributedCacheTests
{
    public sealed record ExampleRequest : IKeyedRequest
    {
        public required string Key { get; init; }
    }

    public sealed record ExampleResponse
    {
        public required int Value { get; init; }
    }

    private static readonly ExampleRequest FakeRequest = new() { Key = "abc" };
    private static readonly ExampleResponse FakeResponse = new() { Value = 123 };

    private static TimeSpan GetFakeTime(int hour) => new(hour, 0, 0);

    private static void SetupInteractionDistributedCacheOptions(
        AutoMocker autoMocker,
        InteractionDistributedCacheOptions<ExampleRequest>? options = null)
    {
        autoMocker.GetMock<IOptions<InteractionDistributedCacheOptions<ExampleRequest>>>()
            .Setup(x => x.Value)
            .Returns(options ?? new InteractionDistributedCacheOptions<ExampleRequest>());
    }

    #region SetAsync(TRequest, object)

    [TestMethod]
    public async Task SetAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.SetAsync(null!, FakeResponse));
    }

    [TestMethod]
    public async Task SetAsync_Throws_WhenResponseArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.SetAsync(FakeRequest, null!));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task SetAsync_Throws_WhenRequestCacheKeyIsInvalid(string cacheKey)
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => cache.SetAsync(new ExampleRequest { Key = cacheKey }, FakeResponse));

        Assert.AreEqual("Invalid cache key.", exception.Message);
    }

    [TestMethod]
    public async Task SetAsync_DoesNotCacheResponse_WhenNullCacheEntryOptions()
    {
        var autoMocker = new AutoMocker();

        SetupInteractionDistributedCacheOptions(autoMocker, new() {
            OverrideCacheEntryOptionsForRequest = (request) => null!,
        });

        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await cache.SetAsync(FakeRequest, FakeResponse);

        autoMocker.Verify<IDistributedCache>(x =>
            x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task SetAsync_CachesSerializedResponse()
    {
        var autoMocker = new AutoMocker();

        SetupInteractionDistributedCacheOptions(autoMocker, new() {
            DefaultAbsoluteExpirationRelativeToNow = GetFakeTime(14),
        });

        autoMocker.GetMock<ICacheEntrySerializer>()
            .Setup(x => x.Serialize(
                It.Is<object>(p => ReferenceEquals(FakeResponse, p))
            ))
            .Returns("123");

        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await cache.SetAsync(FakeRequest, FakeResponse);

        autoMocker.Verify<IDistributedCache>(x =>
            x.SetAsync(
                It.Is<string>(p => p == "Example:abc"),
                It.Is<byte[]>(p => Encoding.UTF8.GetString(p) == "123"),
                It.Is<DistributedCacheEntryOptions>(p => p.AbsoluteExpirationRelativeToNow == GetFakeTime(14)),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    #endregion

    #region GetAsync(TRequest, CancellationToken)

    [TestMethod]
    public async Task GetAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.GetAsync(null!, CancellationToken.None));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task GetAsync_Throws_WhenRequestCacheKeyIsInvalid(string cacheKey)
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => cache.GetAsync(new ExampleRequest { Key = cacheKey }, CancellationToken.None));

        Assert.AreEqual("Invalid cache key.", exception.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsNull_WhenCacheMiss()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsCachedResponse()
    {
        var autoMocker = new AutoMocker();
        SetupInteractionDistributedCacheOptions(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(p => p == "Example:abc"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes("fake-json"));

        autoMocker.GetMock<ICacheEntrySerializer>()
            .Setup(x => x.Deserialize<ExampleResponse>(
                It.Is<string>(p => p == "fake-json")
            ))
            .Returns(FakeResponse);

        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.AreSame(FakeResponse, cachedResponse);
    }

    #endregion

    #region RemoveAsync(string)

    [TestMethod]
    public async Task RemoveAsync_Throws_WhenKeyArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.RemoveAsync(null!));
    }

    [TestMethod]
    public async Task RemoveAsync_Throws_WhenKeyArgumentIsEmpty()
    {
        var autoMocker = new AutoMocker();
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(()
            => cache.RemoveAsync(""));
    }

    [TestMethod]
    public async Task RemoveAsync_RemovesCachedResponse()
    {
        var autoMocker = new AutoMocker();
        SetupInteractionDistributedCacheOptions(autoMocker);
        var cache = autoMocker.CreateInstance<InteractionDistributedCache<ExampleRequest, ExampleResponse>>();

        await cache.RemoveAsync(FakeRequest.Key);

        autoMocker.Verify<IDistributedCache>(x =>
            x.RemoveAsync(
                It.Is<string>(p => p == "Example:abc"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    #endregion
}
