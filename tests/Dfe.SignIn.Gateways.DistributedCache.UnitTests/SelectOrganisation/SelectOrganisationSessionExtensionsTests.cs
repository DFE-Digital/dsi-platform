using System.Text;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Gateways.DistributedCache.SelectOrganisation;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.SelectOrganisation;

// Disable warning "Provide the "DateTimeKind" when creating this object."
// This is a unit test and it's unimportant.
#pragma warning disable S6562

[TestClass]
public sealed class SelectOrganisationSessionDistributedCacheTests
{
    private static readonly SelectOrganisationSessionData FakeSessionData = new() {
        Created = new DateTime(2024, 2, 22),
        Expires = new DateTime(2024, 2, 22) + new TimeSpan(0, 10, 0),
        ClientId = "example-client-id",
        UserId = new Guid("a205d032-e65f-47e0-810c-4ddb424219fd"),
        Prompt = new() {
            Heading = "Which organisation?",
            Hint = "Select one option.",
        },
        OrganisationOptions = [],
        AllowCancel = true,
        CallbackUrl = new Uri("https://example.localhost/callback"),
    };

    #region RetrieveAsync(string)

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => retriever.RetrieveAsync(
                sessionKey: null!
            ));
    }

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(()
            => retriever.RetrieveAsync(""));
    }

    [TestMethod]
    public async Task RetrieveSession_ReturnsNull_WhenSessionDoesNotExist()
    {
        var mocker = new AutoMocker();
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((byte[]?)null);

        var retriever = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        var result = await retriever.RetrieveAsync("example-key");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task RetrieveSession_ReturnsNull_WhenSessionHasExpired()
    {
        var fakeExpiredSession = FakeSessionData with {
            Expires = new DateTime(1955, 1, 1),
        };
        string fakeExpiredSessionJson = new DefaultSessionDataSerializer().Serialize(fakeExpiredSession);

        var mocker = new AutoMocker();
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(param => param == "example-key"),
                CancellationToken.None
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(fakeExpiredSessionJson));

        var retriever = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        var result = await retriever.RetrieveAsync("example-key");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task RetrieveSession_RetrieveSessionFromDistributedCache()
    {
        var fakeSession = FakeSessionData with {
            Expires = DateTime.UtcNow + new TimeSpan(0, 10, 0),
        };
        string fakeSessionJson = new DefaultSessionDataSerializer().Serialize(fakeSession);

        var mocker = new AutoMocker();
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(param => param == "example-key"),
                CancellationToken.None
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(fakeSessionJson));

        var retriever = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        var result = await retriever.RetrieveAsync("example-key");

        Assert.AreEqual(fakeSession, result! with {
            OrganisationOptions = fakeSession.OrganisationOptions,
        });
        CollectionAssert.AreEqual(
            fakeSession.OrganisationOptions.ToArray(),
            result.OrganisationOptions.ToArray()
        );
    }

    #endregion

    #region StoreAsync(string, SelectOrganisationSessionData)

    [TestMethod]
    public async Task StoreAsync_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => storer.StoreAsync(
                sessionKey: null!,
                sessionData: FakeSessionData
            ));
    }

    [TestMethod]
    public async Task StoreAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(()
            => storer.StoreAsync(
                sessionKey: "",
                sessionData: FakeSessionData
            ));
    }

    [TestMethod]
    public async Task StoreAsync_Throws_WhenSessionDataArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => storer.StoreAsync(
                sessionKey: "example-key",
                sessionData: null!
            ));
    }

    [TestMethod]
    public async Task StoreSession_StoreSessionInDistributedCache()
    {
        var mocker = new AutoMocker();
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());

        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await storer.StoreAsync("example-key", FakeSessionData);

        var distributedCacheMock = mocker.GetMock<IDistributedCache>();
        distributedCacheMock.Verify(
            x => x.SetAsync(
                It.Is<string>(param => param == "example-key"),
                It.Is<byte[]>(param => Encoding.UTF8.GetString(param).Contains(
                    "\"clientId\":\"example-client-id\""
                )),
                It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task StoreSession_StoreSessionInDistributedCache_WithAbsoluteCacheExpiration()
    {
        var mocker = new AutoMocker();
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());

        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await storer.StoreAsync("example-key", FakeSessionData);

        var distributedCacheMock = mocker.GetMock<IDistributedCache>();
        distributedCacheMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options =>
                    options.AbsoluteExpiration == FakeSessionData.Expires
                ),
                CancellationToken.None
            ),
            Times.Once
        );
    }

    #endregion

    #region InvalidateAsync(string)

    [TestMethod]
    public async Task InvalidateAsync_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => storer.InvalidateAsync(
                sessionKey: null!
            ));
    }

    [TestMethod]
    public async Task InvalidateAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await Assert.ThrowsExactlyAsync<ArgumentException>(()
            => storer.InvalidateAsync(
                sessionKey: ""
            ));
    }

    [TestMethod]
    public async Task InvalidateAsync_RemovesExpectedKeyFromDistributedCache()
    {
        var mocker = new AutoMocker();

        var storer = mocker.CreateInstance<SelectOrganisationSessionDistributedCache>();

        await storer.InvalidateAsync("example-key");

        var distributedCacheMock = mocker.GetMock<IDistributedCache>();
        distributedCacheMock.Verify(
            x => x.RemoveAsync(
                It.Is<string>(param => param == "example-key"),
                CancellationToken.None
            ),
            Times.Once
        );
    }

    #endregion
}
