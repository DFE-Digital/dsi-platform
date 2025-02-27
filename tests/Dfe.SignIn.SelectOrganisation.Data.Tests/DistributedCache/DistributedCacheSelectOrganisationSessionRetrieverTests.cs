using System.Text;
using Dfe.SignIn.SelectOrganisation.Data.DistributedCache;
using Dfe.SignIn.SelectOrganisation.Data.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.Data.Tests.DistributedCache;

[TestClass]
public sealed class DistributedCacheSelectOrganisationSessionRetrieverTests
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
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>
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
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>
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
            mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>
        );
    }

    #endregion

    #region RetrieveSessionAsync(string)

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsNull()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => retriever.RetrieveSessionAsync(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task RetrieveSession_Throws_WhenSessionKeyArgumentIsEmptyString()
    {
        var mocker = new AutoMocker();
        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>();

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => retriever.RetrieveSessionAsync("")
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

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>();

        var result = await retriever.RetrieveSessionAsync("example-key");

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

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>();

        var result = await retriever.RetrieveSessionAsync("example-key");

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

        var retriever = mocker.CreateInstance<DistributedCacheSelectOrganisationSessionRetriever>();

        var result = await retriever.RetrieveSessionAsync("example-key");

        Assert.AreEqual(fakeSession, result! with {
            OrganisationOptions = fakeSession.OrganisationOptions,
        });
        CollectionAssert.AreEqual(
            fakeSession.OrganisationOptions.ToArray(),
            result.OrganisationOptions.ToArray()
        );
    }

    #endregion
}
