using Dfe.SignIn.Base.Framework.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests.Caching;

[TestClass]
public sealed class InMemoryInteractionCacheTests
{
    public sealed record ExampleRequest : IKeyedRequest
    {
        public required string Key { get; init; }
    }

    public sealed record ExampleResponse
    {
    }

    private static readonly ExampleRequest FakeRequest = new() { Key = "abc" };
    private static readonly ExampleResponse FakeResponse = new();

    private static readonly ExampleRequest FakeOtherRequest = new() { Key = "other" };
    private static readonly ExampleResponse FakeOtherResponse = new();

    private static DateTimeOffset GetFakeTime(int hour) => new(2025, 9, 24, hour, 0, 0, TimeSpan.Zero);

    private static void SetupMemoryCache(AutoMocker autoMocker, Func<int>? getMockHour = null)
    {
        var mockClock = new Mock<Microsoft.Extensions.Internal.ISystemClock>();
        mockClock.Setup(x => x.UtcNow).Returns(() => GetFakeTime(getMockHour?.Invoke() ?? 14));

        autoMocker.Use(new MemoryCache(new MemoryCacheOptions {
            Clock = mockClock.Object,
        }));
    }

    private static void SetupInMemoryInteractionCacheOptions(
        AutoMocker autoMocker,
        InMemoryInteractionCacheOptions<ExampleRequest>? options = null)
    {
        autoMocker.GetMock<IOptions<InMemoryInteractionCacheOptions<ExampleRequest>>>()
            .Setup(x => x.Value)
            .Returns(options ?? new InMemoryInteractionCacheOptions<ExampleRequest>());
    }

    #region SetAsync(TRequest, object)

    [TestMethod]
    public async Task SetAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.SetAsync(null!, FakeResponse));
    }

    [TestMethod]
    public async Task SetAsync_Throws_WhenResponseArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

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
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => cache.SetAsync(new ExampleRequest { Key = cacheKey }, FakeResponse));

        Assert.AreEqual("Invalid cache key.", exception.Message);
    }

    [TestMethod]
    public async Task SetAsync_DoesNotCacheResponse_WhenNullCacheEntryOptions()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        SetupInMemoryInteractionCacheOptions(autoMocker, new() {
            OverrideCacheEntryOptionsForRequest = (request) => null!,
        });
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await cache.SetAsync(FakeRequest, FakeResponse);

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    #endregion

    #region GetAsync(TRequest, CancellationToken)

    [TestMethod]
    public async Task GetAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

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
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => cache.GetAsync(new ExampleRequest { Key = cacheKey }, CancellationToken.None));

        Assert.AreEqual("Invalid cache key.", exception.Message);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsNull_WhenCacheHasExpired()
    {
        var autoMocker = new AutoMocker();

        int fakeHourNow = 14; // It's 2pm.
        SetupMemoryCache(autoMocker, () => fakeHourNow);

        SetupInMemoryInteractionCacheOptions(autoMocker, new() {
            DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
        });

        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();
        await cache.SetAsync(FakeRequest, FakeResponse);

        fakeHourNow += 4; // Time travel into the future by 4 hours.

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsNull_WhenCacheMiss()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsCachedResponse_WhenCacheHasNotExpired()
    {
        var autoMocker = new AutoMocker();

        int fakeHourNow = 14; // It's 2pm.
        SetupMemoryCache(autoMocker, () => fakeHourNow);

        SetupInMemoryInteractionCacheOptions(autoMocker, new() {
            DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
        });

        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();
        await cache.SetAsync(FakeRequest, FakeResponse);

        fakeHourNow += 1; // Time travel into the future by 1 hour.

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.AreSame(FakeResponse, cachedResponse);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsNull_WhenCacheMissOnNonEmptyCache()
    {
        var autoMocker = new AutoMocker();

        int fakeHourNow = 14; // It's 2pm.
        SetupMemoryCache(autoMocker, () => fakeHourNow);

        SetupInMemoryInteractionCacheOptions(autoMocker, new() {
            DefaultAbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
        });

        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();
        await cache.SetAsync(FakeRequest, FakeResponse);

        fakeHourNow += 2; // Time travel into the future by 2 hours.

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsCachedResponse()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        SetupInMemoryInteractionCacheOptions(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await cache.SetAsync(FakeRequest, FakeResponse);

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.AreSame(FakeResponse, cachedResponse);
    }

    #endregion

    #region RemoveAsync(string)

    [TestMethod]
    public async Task RemoveAsync_Throws_WhenKeyArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => cache.RemoveAsync(null!));
    }

    [TestMethod]
    public async Task RemoveAsync_Throws_WhenKeyArgumentIsEmpty()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(()
            => cache.RemoveAsync(""));
    }

    [TestMethod]
    public async Task RemoveAsync_RemovesCachedResponse()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        SetupInMemoryInteractionCacheOptions(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await cache.SetAsync(FakeRequest, FakeResponse);

        await cache.RemoveAsync(FakeRequest.Key);

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
    }

    [TestMethod]
    public async Task RemoveAsync_RetainsOtherCachedResponses()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        SetupInMemoryInteractionCacheOptions(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await cache.SetAsync(FakeOtherRequest, FakeOtherResponse);
        await cache.SetAsync(FakeRequest, FakeResponse);

        await cache.RemoveAsync(FakeRequest.Key);

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);
        var otherCachedResponse = await cache.GetAsync(FakeOtherRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
        Assert.AreSame(FakeOtherResponse, otherCachedResponse);
    }

    #endregion

    #region ClearAsync()

    [TestMethod]
    public async Task ClearAsync_RemovesAllCachedResponses()
    {
        var autoMocker = new AutoMocker();
        SetupMemoryCache(autoMocker);
        SetupInMemoryInteractionCacheOptions(autoMocker);
        var cache = autoMocker.CreateInstance<InMemoryInteractionCache<ExampleRequest>>();

        await cache.SetAsync(FakeOtherRequest, FakeOtherResponse);
        await cache.SetAsync(FakeRequest, FakeResponse);

        await cache.ClearAsync();

        var cachedResponse = await cache.GetAsync(FakeRequest, CancellationToken.None);
        var otherCachedResponse = await cache.GetAsync(FakeOtherRequest, CancellationToken.None);

        Assert.IsNull(cachedResponse);
        Assert.IsNull(otherCachedResponse);
    }

    #endregion
}
