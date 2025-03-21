using System.Net;
using Dfe.SignIn.Core.PublicModels.PublicApiSigning;
using Dfe.SignIn.PublicApiClient.PublicApiSigning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Moq.Protected;

namespace Dfe.SignIn.PublicApiClient.UnitTests;

[TestClass]
[TestCategory("LongRunningTest")]
public sealed class PublicKeyCacheTests
{
    private static readonly WellKnownPublicKey FakeWellKnownPublicKey = new() {
        Kid = "FakePublicKey1",
        Kty = "RSA",
        Use = "sig",
        Alg = "RS256",
        N = "" +
            "wg3F2vOvN5clme1SpJ6RCr_BhcRyIrubkT7zjZh-tOcr1BFZRDUkpIgan" +
            "3POdMBsuzRqHlA6f1pXeSeS-Ba5wEiyU6K3nkXeQAc-hle-Vz-QVDLFji" +
            "IbWAz6qeKEPPRr8kKAOEDUebKPbIcroqxYPV6EkJ_niOU5_yruw9cn3br" +
            "uj1JFtiFa1eHMMInmGtXBvmcgiTsw-3dp5SHwZahpGIF7z7XnqCTemgbH" +
            "hm08EVGPIPp_mptzG7i0qGdd934SuCaJoVhoNPM5UqtXfMAwH0Rnq1qTv" +
            "ZX-UX1x688bJiMgndhUXddISNBaGYxl8-TZY_LUAa_whicY4cg1RJopZw",
        E = "AQAB",
        Ed = 1941364297,
    };

#pragma warning disable JSON002 // Probable JSON string detected
    private static readonly string FakeEmptyKeysHttpResponse = """
        {
            "keys": []
        }
    """;

    private static readonly string FakeKeyHttpResponse = $$"""
        {
            "keys": [
                {
                    "kid": "{{FakeWellKnownPublicKey.Kid}}",
                    "kty": "{{FakeWellKnownPublicKey.Kty}}",
                    "n": "{{FakeWellKnownPublicKey.N}}",
                    "alg": "{{FakeWellKnownPublicKey.Alg}}",
                    "e": "{{FakeWellKnownPublicKey.E}}",
                    "ed": {{FakeWellKnownPublicKey.Ed}},
                    "use": "{{FakeWellKnownPublicKey.Use}}"
                }
            ]
        }
    """;

    private static readonly string FakeTwoKeysHttpResponse = $$"""
        {
            "keys": [
                {
                    "kid": "{{FakeWellKnownPublicKey.Kid}}",
                    "kty": "{{FakeWellKnownPublicKey.Kty}}",
                    "n": "{{FakeWellKnownPublicKey.N}}",
                    "alg": "{{FakeWellKnownPublicKey.Alg}}",
                    "e": "{{FakeWellKnownPublicKey.E}}",
                    "ed": {{FakeWellKnownPublicKey.Ed}},
                    "use": "{{FakeWellKnownPublicKey.Use}}"
                },
                {
                    "kid": "FakePublicKeyNew",
                    "kty": "{{FakeWellKnownPublicKey.Kty}}",
                    "n": "{{FakeWellKnownPublicKey.N}}",
                    "alg": "{{FakeWellKnownPublicKey.Alg}}",
                    "e": "{{FakeWellKnownPublicKey.E}}",
                    "ed": {{FakeWellKnownPublicKey.Ed}},
                    "use": "{{FakeWellKnownPublicKey.Use}}"
                }
            ]
        }
    """;
#pragma warning restore JSON002 // Probable JSON string detected

    private static void UseMockedOptions(
        AutoMocker autoMocker,
        int ttlMilliseconds = 500,
        int maximumRefreshIntervalMilliseconds = 250)
    {
        autoMocker.GetMock<IOptions<DfePublicApiOptions>>()
            .Setup(x => x.Value)
            .Returns(new DfePublicApiOptions {
                BaseAddress = new Uri("https://unit-tests.localhost"),
                ApiSecret = "<api_secret>",
                ClientId = "<client_id>",
            });

        autoMocker.GetMock<IOptions<PublicKeyCacheOptions>>()
            .Setup(x => x.Value)
            .Returns(new PublicKeyCacheOptions {
                TTL = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: ttlMilliseconds),
                MaximumRefreshInterval = new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: maximumRefreshIntervalMilliseconds),
            });
    }

    private static void UseHttpClientWithFakeResponse(AutoMocker autoMocker, string fakeHttpResponse)
    {
        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeHttpResponse)
            });

        autoMocker.Use(new HttpClient(mockHttp.Object));
    }

    private static void UseHttpClientWithTwoFakePublicKeysNextTime(AutoMocker autoMocker)
    {
        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();

        mockHttp.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(FakeKeyHttpResponse)
            })
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(FakeTwoKeysHttpResponse)
            });

        autoMocker.Use(new HttpClient(mockHttp.Object));
    }

    private static void UseHttpClientWithUnexpectedException(AutoMocker autoMocker)
    {
        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();

        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Throws<HttpRequestException>();

        autoMocker.Use(new HttpClient(mockHttp.Object));
    }

    #region GetPublicKeyAsync(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task GetPublicKeyAsync_Throws_WhenKeyIdArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync(null!);
    }

    [DataRow("")]
    [DataRow("   ")]
    [DataTestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task GetPublicKeyAsync_Throws_WhenKeyIdArgumentIsEmpty(string invalidKeyId)
    {
        var autoMocker = new AutoMocker();

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync(invalidKeyId);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_FetchesPublicKeysFromPublicApiOnce()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);

        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();
        autoMocker.Use(new HttpClient(mockHttp.Object));

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey2");

        mockHttp.Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == new Uri("https://unit-tests.localhost/v2/.well-known/keys")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_FetchesFreshPublicKeys_WhenKeyIsFound()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, FakeKeyHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        await Task.Delay(millisecondsDelay: 15);
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        autoMocker.GetMock<HttpMessageHandler>().Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == new Uri("https://unit-tests.localhost/v2/.well-known/keys")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_FetchesFreshPublicKeys_WhenTtlHasElapsed()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker, ttlMilliseconds: 10, maximumRefreshIntervalMilliseconds: 0);
        UseHttpClientWithFakeResponse(autoMocker, FakeKeyHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        await Task.Delay(millisecondsDelay: 15);
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        autoMocker.GetMock<HttpMessageHandler>().Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == new Uri("https://unit-tests.localhost/v2/.well-known/keys")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_FetchesFreshPublicKeys_WhenMaximumRefreshIntervalHasElapsed()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker, maximumRefreshIntervalMilliseconds: 12);

        var mockHttp = autoMocker.GetMock<HttpMessageHandler>();
        autoMocker.Use(new HttpClient(mockHttp.Object));

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        await Task.Delay(millisecondsDelay: 15);
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey2");
        await publicKeyCache.GetPublicKeyAsync("FakePublicKey2");

        mockHttp.Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == new Uri("https://unit-tests.localhost/v2/.well-known/keys")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_ReturnsNull_WhenPublicApiReturnsNull()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, "null");

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.IsNull(cacheEntry);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_ReturnsNull_WhenPublicApiReturnsNoKeys()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, FakeEmptyKeysHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.IsNull(cacheEntry);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_LogsWarning_WhenPublicApiReturnsNoKeys()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, FakeEmptyKeysHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        string expectedMessage = "Was unable to retrieve public keys from DfE Sign-in Public API.";

        autoMocker.GetMock<ILogger<PublicKeyCache>>()
            .Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => string.Equals(expectedMessage, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<NoPublicKeysWereFoundException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_ReturnsNull_WhenUnexpectedExceptionOccurs()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithUnexpectedException(autoMocker);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.IsNull(cacheEntry);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_LogsWarning_WhenUnexpectedExceptionOccurs()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithUnexpectedException(autoMocker);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        string expectedMessage = "Was unable to retrieve public keys from DfE Sign-in Public API.";

        autoMocker.GetMock<ILogger<PublicKeyCache>>()
            .Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => string.Equals(expectedMessage, o.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_ReturnsCacheEntry_WhenPublicKeyIsFound()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, FakeKeyHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.IsNotNull(cacheEntry);
        Assert.AreEqual(FakeWellKnownPublicKey, cacheEntry.Key);
        Assert.IsNotNull(cacheEntry.RSA);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_ReturnsSameCacheEntry_WhenCalledMultipleTimes()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseHttpClientWithFakeResponse(autoMocker, FakeKeyHttpResponse);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry1 = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        var cacheEntry2 = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.AreSame(cacheEntry1, cacheEntry2);
    }

    [TestMethod]
    public async Task GetPublicKeyAsync_RetainsOriginalCacheEntry_WhenNewKeysAreLoaded()
    {
        var autoMocker = new AutoMocker();
        // Allow new public keys to be loaded immediately so that we can verify
        // that old keys are retained when new ones are retrieved.
        UseMockedOptions(autoMocker, maximumRefreshIntervalMilliseconds: 0);
        UseHttpClientWithTwoFakePublicKeysNextTime(autoMocker);

        var publicKeyCache = autoMocker.CreateInstance<PublicKeyCache>();

        var cacheEntry1 = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");
        var newCacheEntry = await publicKeyCache.GetPublicKeyAsync("FakePublicKeyNew");
        var cacheEntry2 = await publicKeyCache.GetPublicKeyAsync("FakePublicKey1");

        Assert.AreSame(cacheEntry1, cacheEntry2);
        Assert.IsNotNull(newCacheEntry);
    }

    #endregion
}
