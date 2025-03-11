using System.Text;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.SelectOrganisation.SessionData.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.SessionData.UnitTests;

[TestClass]
public sealed class DistributedCacheSelectOrganisationSessionRepositoryTests
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
        CallbackUrl = new Uri("https://example.localhost/callback"),
        DetailLevel = OrganisationDetailLevel.Basic,
    };

    private static readonly SelectOrganisationSessionCacheOptions fakeOptions = new() {
        CacheKeyPrefix = "example-prefix:",
    };

    #region RetrieveAsync(string)

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => retriever.RetrieveAsync(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => retriever.RetrieveAsync("")
        );
    }

    [TestMethod]
    public async Task RetrieveSession_ReturnsNull_WhenSessionDoesNotExist()
    {
        var mocker = new AutoMocker();
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((byte[]?)null);

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

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
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(param => param == "example-prefix:example-key"),
                CancellationToken.None
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(fakeExpiredSessionJson));

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

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
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());
        mocker.GetMock<IDistributedCache>()
            .Setup(x => x.GetAsync(
                It.Is<string>(param => param == "example-prefix:example-key"),
                CancellationToken.None
            ))
            .ReturnsAsync(Encoding.UTF8.GetBytes(fakeSessionJson));

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

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
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.StoreAsync(null, FakeSessionData)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task StoreAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => storer.StoreAsync("", FakeSessionData)
        );
    }

    [TestMethod]
    public async Task StoreAsync_Throws_WhenSessionDataArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.StoreAsync("example-key", null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task StoreSession_StoreSessionInDistributedCache()
    {
        var mocker = new AutoMocker();
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);
        mocker.Use<ISessionDataSerializer>(new DefaultSessionDataSerializer());

        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

        await storer.StoreAsync("example-key", FakeSessionData);

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

    #region InvalidateAsync(string)

    [TestMethod]
    public async Task InvalidateAsync_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => storer.InvalidateAsync(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task InvalidateAsync_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => storer.InvalidateAsync("")
        );
    }

    [TestMethod]
    public async Task InvalidateAsync_RemovesExpectedKeyFromDistributedCache()
    {
        var mocker = new AutoMocker();
        mocker.Use<IOptions<SelectOrganisationSessionCacheOptions>>(fakeOptions);

        var storer = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRepository>();

        await storer.InvalidateAsync("example-key");

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
