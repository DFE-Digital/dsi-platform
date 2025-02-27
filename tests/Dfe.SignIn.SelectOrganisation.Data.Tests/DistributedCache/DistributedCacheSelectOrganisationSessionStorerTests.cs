using System.Text;
using Dfe.SignIn.SelectOrganisation.Data.DistributedCache;
using Dfe.SignIn.SelectOrganisation.Data.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.Data.Tests.DistributedCache;

[TestClass]
public sealed class DistributedCacheSelectOrganisationSessionStorerTests
{
    private static readonly SelectOrganisationSessionData FakeSessionData = new() {
        Created = new DateTime(2024, 2, 22),
        Expires = new DateTime(2024, 2, 22) + new TimeSpan(0, 10, 0),
        CallbackUrl = new Uri("https://example.localhost/callback"),
        ClientId = "example-client-id",
        UserId = new Guid("a205d032-e65f-47e0-810c-4ddb424219fd"),
        Prompt = new() {
            Heading = "Which organisation?",
            Hint = "Select one option.",
        },
        OrganisationOptions = [],
    };

    private static readonly SelectOrganisationSessionCacheOptions fakeOptions = new() {
        CacheKeyPrefix = "example-prefix:",
    };

    #region Constructor()

    [TestMethod]
    public void Constructor_Throws_WhenOptionsAccessorArgumentIsNull()
    {
        var mocker = new AutoMocker();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        mocker.Use(typeof(IOptions<SelectOrganisationSessionCacheOptions>), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.ThrowsException<ArgumentNullException>(
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>
        );
    }

    [TestMethod]
    public void Constructor_Throws_WhenCacheArgumentIsNull()
    {
        var mocker = new AutoMocker();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        mocker.Use(typeof(IDistributedCache), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.ThrowsException<ArgumentNullException>(
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>
        );
    }

    [TestMethod]
    public void Constructor_Throws_WhenSerializerArgumentIsNull()
    {
        var mocker = new AutoMocker();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        mocker.Use(typeof(ISessionDataSerializer), null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.ThrowsException<ArgumentNullException>(
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>
        );
    }

    #endregion

    #region StoreSessionAsync(string, SelectOrganisationSessionData)

    [TestMethod]
    public async Task StoreSessionAsync_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.StoreSessionAsync(null, FakeSessionData)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task StoreSessionAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => storer.StoreSessionAsync("", FakeSessionData)
        );
    }

    [TestMethod]
    public async Task StoreSessionAsync_Throws_WhenSessionDataArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.StoreSessionAsync("example-key", null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task StoreSession_StoreSessionInDistributedCache()
    {
        var mocker = new AutoMocker();
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());

        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

        await storer.StoreSessionAsync("example-key", FakeSessionData);

        var distributedCacheMock = mocker.GetMock<IDistributedCache>();
        distributedCacheMock.Verify(
            x => x.SetAsync(
                It.Is<string>(param => param == "example-prefix:example-key"),
                It.Is<byte[]>(param => Encoding.UTF8.GetString(param).Contains(
                    "\"clientId\":\"example-client-id\""
                )),
                It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None
            ),
            Times.Once
        );
    }

    #endregion

    #region InvalidateSessionAsync(string)

    [TestMethod]
    public async Task InvalidateSessionAsync_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.InvalidateSessionAsync(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task InvalidateSessionAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => storer.InvalidateSessionAsync("")
        );
    }

    [TestMethod]
    public async Task InvalidateSessionAsync_RemovesExpectedKeyFromDistributedCache()
    {
        var mocker = new AutoMocker();
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);

        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionStorer>();

        await storer.InvalidateSessionAsync("example-key");

        var distributedCacheMock = mocker.GetMock<IDistributedCache>();
        distributedCacheMock.Verify(
            x => x.RemoveAsync(
                It.Is<string>(param => param == "example-prefix:example-key"),
                CancellationToken.None
            ),
            Times.Once
        );
    }

    #endregion
}
