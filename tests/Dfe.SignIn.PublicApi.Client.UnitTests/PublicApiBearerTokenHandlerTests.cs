using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiBearerTokenHandlerTests
{
    private static void UseMockedOptions(AutoMocker autoMocker, DfePublicApiOptions? options = null)
    {
        autoMocker.GetMock<IOptions<DfePublicApiOptions>>()
            .Setup(x => x.Value)
            .Returns(options ?? new DfePublicApiOptions {
                ApiSecret = "fake api secret for unit testing purposes",
                ClientId = "<client_id>",
                Audience = "<audience>",
            });
    }

    private static void UseMemoryCache(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptions<MemoryCacheOptions>>()
            .Setup(x => x.Value)
            .Returns(new MemoryCacheOptions());

        autoMocker.With<IMemoryCache, MemoryCache>();
    }

    private static PublicApiBearerTokenHandler CreatePublicApiBearerTokenHandler(AutoMocker autoMocker)
    {
        var handler = autoMocker.CreateInstance<PublicApiBearerTokenHandler>();
        handler.InnerHandler = autoMocker.Get<HttpMessageHandler>();
        return handler;
    }

    #region Send(HttpRequestMessage, CancellationToken)

    [TestMethod]
    public void Send_AddsAuthorizationHeader()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseMemoryCache(autoMocker);

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);
        var invoker = new HttpMessageInvoker(handler);

        var fakeHttpRequest = new HttpRequestMessage(HttpMethod.Get, "v2/.well-known/keys");
        invoker.Send(fakeHttpRequest, CancellationToken.None);

        Assert.IsNotNull(fakeHttpRequest.Headers.Authorization);
    }

    #endregion

    #region SendAsync(HttpRequestMessage, CancellationToken)

    [TestMethod]
    public async Task SendAsync_AddsAuthorizationHeader()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseMemoryCache(autoMocker);

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);
        var invoker = new HttpMessageInvoker(handler);

        var fakeHttpRequest = new HttpRequestMessage(HttpMethod.Get, "v2/.well-known/keys");
        await invoker.SendAsync(fakeHttpRequest, CancellationToken.None);

        Assert.IsNotNull(fakeHttpRequest.Headers.Authorization);
    }

    #endregion

    #region CreateAuthorizationHeader()

    [DataRow(null)]
    [DataRow("   ")]
    [DataTestMethod]
    public void CreateAuthorizationHeader_Throws_WhenApiSecretOptionIsMissing(string fakeApiSecret)
    {
        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = fakeApiSecret,
            ClientId = "<client_id>",
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var exception = Assert.ThrowsException<InvalidOperationException>(
            handler.CreateAuthorizationHeader
        );
        Assert.AreEqual("Invalid DfE Sign-in Public API secret.", exception.Message);
    }

    [DataRow(null)]
    [DataRow("   ")]
    [DataTestMethod]
    public void CreateAuthorizationHeader_Throws_WhenClientIdOptionIsMissing(string fakeClientId)
    {
        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = "fake api secret for unit testing purposes",
            ClientId = fakeClientId,
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var exception = Assert.ThrowsException<InvalidOperationException>(
            handler.CreateAuthorizationHeader
        );
        Assert.AreEqual("Invalid DfE Sign-in Public API client ID.", exception.Message);
    }

    [DataRow(null)]
    [DataRow("   ")]
    [DataTestMethod]
    public void CreateAuthorizationHeader_Throws_WhenAudienceOptionIsMissing(string fakeAudience)
    {
        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = "fake api secret for unit testing purposes",
            ClientId = "<client_id>",
            Audience = fakeAudience,
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var exception = Assert.ThrowsException<InvalidOperationException>(
            handler.CreateAuthorizationHeader
        );
        Assert.AreEqual("Invalid DfE Sign-in Public API service audience.", exception.Message);
    }

    [TestMethod]
    public void CreateAuthorizationHeader_HasBearerScheme()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseMemoryCache(autoMocker);

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header = handler.CreateAuthorizationHeader();

        Assert.AreEqual("Bearer", header.Scheme);
    }

    [TestMethod]
    public void CreateAuthorizationHeader_HasNonEmptyParameter()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseMemoryCache(autoMocker);

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header = handler.CreateAuthorizationHeader();

        Assert.IsNotNull(header.Parameter);
        Assert.AreNotEqual(string.Empty, header.Parameter);
    }

    [TestMethod]
    public void CreateAuthorizationHeader_ReturnsCachedToken_WhenCalledAgain()
    {
        var autoMocker = new AutoMocker();
        UseMockedOptions(autoMocker);
        UseMemoryCache(autoMocker);

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header1 = handler.CreateAuthorizationHeader();
        var header2 = handler.CreateAuthorizationHeader();

        Assert.AreEqual(header2.Scheme, header1.Scheme);
        Assert.AreEqual(header2.Parameter, header1.Parameter);
    }

    [TestMethod]
    [TestCategory("LongRunningTest")]
    public async Task CreateAuthorizationHeader_ReturnsNewToken_WhenCacheHasExpired()
    {
        // This test caches the token for 100ms and then waits for 1000ms to verify
        // that a new token is issued. This test becomes unstable with smaller timings.

        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = "fake api secret for unit testing purposes",
            ClientId = "<client_id>",
            BearerTokenTtlInMinutes = TimeSpan.FromMilliseconds(100).TotalMinutes,
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header1 = handler.CreateAuthorizationHeader();
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        var header2 = handler.CreateAuthorizationHeader();

        Assert.AreEqual(header2.Scheme, header1.Scheme);
        Assert.AreNotEqual(header2.Parameter, header1.Parameter);
    }

    [TestMethod]
    public void CreateAuthorizationHeader_DoesNotIncludeExpiryClaim_WhenBearerTokenTtlInMinutesIsNull()
    {
        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = "fake api secret for unit testing purposes",
            ClientId = "<client_id>",
            BearerTokenTtlInMinutes = 0,
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header = handler.CreateAuthorizationHeader();
        string jwtDataJson = Encoding.UTF8.GetString(
            Base64UrlEncoder.DecodeBytes(header.Parameter!.Split(".")[1])
        );
        var jwtData = JsonNode.Parse(jwtDataJson)!;

        Assert.IsNull(jwtData["exp"]);
    }

    [TestMethod]
    public void CreateAuthorizationHeader_IncludesExpiryClaim_WhenBearerTokenTtlInMinutesIsSpecified()
    {
        var autoMocker = new AutoMocker();
        UseMemoryCache(autoMocker);

        UseMockedOptions(autoMocker, new DfePublicApiOptions {
            ApiSecret = "fake api secret for unit testing purposes",
            ClientId = "<client_id>",
            BearerTokenTtlInMinutes = 10,
        });

        var handler = CreatePublicApiBearerTokenHandler(autoMocker);

        var header = handler.CreateAuthorizationHeader();
        string jwtDataJson = Encoding.UTF8.GetString(
            Base64UrlEncoder.DecodeBytes(header.Parameter!.Split(".")[1])
        );
        var jwtData = JsonNode.Parse(jwtDataJson)!;

        Assert.IsNotNull(jwtData["exp"]);

        long expClaim = jwtData["exp"]!.GetValue<long>();

        bool tokenExpiresInTheFuture = expClaim > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.IsTrue(tokenExpiresInTheFuture);

        bool tokenValidNearEndOfExpiry = expClaim > DateTimeOffset.UtcNow.AddMinutes(9).ToUnixTimeSeconds();
        Assert.IsTrue(tokenValidNearEndOfExpiry);

        bool tokenExpiresAfterBearerTokenTtl = expClaim < DateTimeOffset.UtcNow.AddMinutes(11).ToUnixTimeSeconds();
        Assert.IsTrue(tokenExpiresAfterBearerTokenTtl);
    }

    #endregion
}
