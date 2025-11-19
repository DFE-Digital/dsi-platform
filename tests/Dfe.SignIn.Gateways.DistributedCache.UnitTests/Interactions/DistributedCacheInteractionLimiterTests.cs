using System.Text;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.Interactions;

[TestClass]
public sealed class DistributedCacheInteractionLimiterTests
{
    private sealed record FakeRequest : IKeyedRequest
    {
        public string Key => "FakeKey";
    }

    private static DistributedCacheInteractionLimiter CreateLimiter(AutoMocker autoMocker)
    {
        autoMocker.Use<TimeProvider>(new MockTimeProvider(new DateTimeOffset(2025, 11, 18, 17, 56, 45, TimeSpan.Zero)));

        autoMocker.GetMock<IOptionsMonitor<DistributedCacheInteractionLimiterOptions>>()
            .Setup(x => x.Get(
                It.Is<string>(key => key == nameof(FakeRequest))
            ))
            .Returns(new DistributedCacheInteractionLimiterOptions {
                TimePeriodInSeconds = 1800,
                InteractionsPerTimePeriod = 3,
            });

        return autoMocker.CreateInstance<DistributedCacheInteractionLimiter>();
    }

    private static void CaptureSetCacheEntry(AutoMocker autoMocker, Action<DistributedCacheInteractionLimiter.CacheEntry?, DistributedCacheEntryOptions?> capture)
    {
        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.SetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (_, value, options, _) => capture(
                    JsonSerializer.Deserialize<DistributedCacheInteractionLimiter.CacheEntry>(value),
                    options
                ));
    }

    #region LimitActionAsync(IKeyedRequest)

    [TestMethod]
    public async Task LimitActionAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => limiter.LimitActionAsync(null!));
    }

    [TestMethod]
    public async Task LimitActionAsync_DoesNotReject_WhenCounterNotPresent()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.FromResult<byte[]?>(null));

        var response = await limiter.LimitActionAsync(new FakeRequest());

        Assert.IsFalse(response.WasRejected);
    }

    [TestMethod]
    public async Task LimitActionAsync_DoesNotReject_WhenCounterIsOne()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(/*lang=json,strict*/ """
                { "Counter": 1, "Expires": "2025-11-18T17:56:45.0" }
            """));

        var response = await limiter.LimitActionAsync(new FakeRequest());

        Assert.IsFalse(response.WasRejected);
    }

    [TestMethod]
    public async Task LimitActionAsync_Rejects_WhenCounterAtLimit()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(/*lang=json,strict*/ """
                { "Counter": 3, "Expires": "2025-11-18T17:56:45.0" }
            """));

        var response = await limiter.LimitActionAsync(new FakeRequest());

        Assert.IsTrue(response.WasRejected);
    }

    [TestMethod]
    public async Task LimitActionAsync_SetsNewCounterToOne()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.FromResult<byte[]?>(null));

        DistributedCacheInteractionLimiter.CacheEntry? capturedCacheEntry = null;
        DistributedCacheEntryOptions? capturedOptions = null;
        CaptureSetCacheEntry(autoMocker, (entry, options) => {
            capturedCacheEntry = entry;
            capturedOptions = options;
        });

        await limiter.LimitActionAsync(new FakeRequest());

        Assert.IsNotNull(capturedCacheEntry);
        Assert.IsNotNull(capturedOptions);

        // Starts counter at 1.
        Assert.AreEqual(1, capturedCacheEntry.Counter);

        // Sets expiration time to Now + configured time period (30 mins).
        Assert.AreEqual(new DateTime(2025, 11, 18, 18, 26, 45, DateTimeKind.Utc), capturedCacheEntry.Expires);
        Assert.AreEqual(new DateTimeOffset(2025, 11, 18, 18, 26, 45, TimeSpan.Zero), capturedOptions.AbsoluteExpiration);
    }

    [TestMethod]
    public async Task LimitActionAsync_IncrementsExistingCounter()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        autoMocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(/*lang=json,strict*/ """
                { "Counter": 2, "Expires": "2025-11-18T17:01:45.0" }
            """));

        DistributedCacheInteractionLimiter.CacheEntry? capturedCacheEntry = null;
        DistributedCacheEntryOptions? capturedOptions = null;
        CaptureSetCacheEntry(autoMocker, (entry, options) => {
            capturedCacheEntry = entry;
            capturedOptions = options;
        });

        await limiter.LimitActionAsync(new FakeRequest());

        Assert.IsNotNull(capturedCacheEntry);
        Assert.IsNotNull(capturedOptions);

        // Increments existing counter.
        Assert.AreEqual(3, capturedCacheEntry.Counter);

        // Preserves existing expiration time.
        Assert.AreEqual(new DateTime(2025, 11, 18, 17, 01, 45, DateTimeKind.Utc), capturedCacheEntry.Expires);
        Assert.AreEqual(new DateTimeOffset(2025, 11, 18, 17, 01, 45, TimeSpan.Zero), capturedOptions.AbsoluteExpiration);
    }

    #endregion

    #region ResetLimitAsync(IKeyedRequest)

    [TestMethod]
    public async Task ResetLimitAsync_Throws_WhenRequestArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => limiter.ResetLimitAsync(null!));
    }

    [TestMethod]
    public async Task ResetLimitAsync_RemovesAssociatedCacheEntry()
    {
        var autoMocker = new AutoMocker();
        var limiter = CreateLimiter(autoMocker);

        await limiter.ResetLimitAsync(new FakeRequest());

        autoMocker.Verify<IDistributedCache>(x => x.RemoveAsync(
            It.Is<string>(key => key == "Limiter:FakeRequest:FakeKey"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion
}
