using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Contracts.Organisations;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class ActiveOrganisationSessionProviderTests
{
    private static readonly OrganisationDetails FakeOrganisation = new() {
        Id = new("55fbf30e-b30e-4cdd-bb69-feb0adee16b6"),
        Name = "Example Organisation",
    };

    private static readonly string FakeOrganisationJson = JsonSerializer.Serialize(FakeOrganisation);

    private const string FakeSessionId = "9dbde47d-2a9e-4bcf-818c-75c2fcc9ee25";
    private const string FakeUserId = "ca5a688b-0b7a-400b-8303-5f62b197526e";

    private static Mock<IHttpContext> CreateMockHttpContext(AutoMocker autoMocker)
    {
        var mockAbstractionContext = autoMocker.GetMock<IHttpContext>();

        var mockContext = autoMocker.GetMock<HttpContext>();
        mockAbstractionContext.Setup(mock => mock.Inner).Returns(mockContext.Object);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, FakeSessionId),
                new(DsiClaimTypes.UserId, FakeUserId),
            ])
        );
        mockContext.Setup(mock => mock.User).Returns(user);

        var mockSession = autoMocker.GetMock<ISession>();
        mockContext.Setup(mock => mock.Session).Returns(mockSession.Object);

        return mockAbstractionContext;
    }

    #region SetActiveOrganisationAsync(IHttpContext, OrganisationDetails?)

    private static void AssertSessionSetString(AutoMocker autoMocker, string expectedKey, string expectedValue)
    {
        autoMocker.Verify<ISession>(mock =>
            mock.Set(
                It.Is<string>(key => key == expectedKey),
                It.Is<byte[]>(value => Encoding.UTF8.GetString(value) == expectedValue)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_Throws_WhenContextArgumentIsNull()
    {
        var provider = new ActiveOrganisationSessionProvider();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => provider.SetActiveOrganisationAsync(null!, null));
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_SetsAssociatedSidKeyInSession()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        var provider = new ActiveOrganisationSessionProvider();

        await provider.SetActiveOrganisationAsync(mockContext.Object, null);

        AssertSessionSetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, FakeSessionId);
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_SetsOrganisationInSession_WhenNull()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        var provider = new ActiveOrganisationSessionProvider();

        await provider.SetActiveOrganisationAsync(mockContext.Object, null);

        AssertSessionSetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, "null");
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_SetsOrganisationInSession()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        var provider = new ActiveOrganisationSessionProvider();

        await provider.SetActiveOrganisationAsync(mockContext.Object, FakeOrganisation);

        AssertSessionSetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, FakeOrganisationJson);
    }

    #endregion

    #region GetActiveOrganisationStateAsync(IHttpContext)

    private static void MockSessionGetString(AutoMocker autoMocker, string expectedKey, string expectedValue)
    {
        byte[] output = Encoding.UTF8.GetBytes(expectedValue);
        autoMocker.GetMock<ISession>()
            .Setup(mock => mock.TryGetValue(
                It.Is<string>(key => key == expectedKey),
                out output
            ))
            .Returns(true);
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_Throws_WhenContextArgumentIsNull()
    {
        var provider = new ActiveOrganisationSessionProvider();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => provider.GetActiveOrganisationStateAsync(null!));
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_ClearsState_WhenSidMismatched()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, "1cd467db-1c7a-4161-aeaf-0612b5e20386");
        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, FakeOrganisationJson);

        var provider = new ActiveOrganisationSessionProvider();

        var result = await provider.GetActiveOrganisationStateAsync(mockContext.Object);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_ClearsState_WhenMissingActiveOrganisation()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, FakeSessionId);
        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, "");

        var provider = new ActiveOrganisationSessionProvider();

        var result = await provider.GetActiveOrganisationStateAsync(mockContext.Object);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_ClearsState_WhenJsonIsMalformed()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, FakeSessionId);
        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, "{,");

        var provider = new ActiveOrganisationSessionProvider();

        var result = await provider.GetActiveOrganisationStateAsync(mockContext.Object);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_GetsOrganisationState_WhenNull()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, FakeSessionId);
        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, "null");

        var provider = new ActiveOrganisationSessionProvider();

        var result = await provider.GetActiveOrganisationStateAsync(mockContext.Object);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Organisation);
    }

    [TestMethod]
    public async Task GetActiveOrganisationStateAsync_GetsOrganisationState()
    {
        var autoMocker = new AutoMocker();
        var mockContext = CreateMockHttpContext(autoMocker);

        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.AssociatedSidKey, FakeSessionId);
        MockSessionGetString(autoMocker, ActiveOrganisationSessionProvider.OrganisationKey, FakeOrganisationJson);

        var provider = new ActiveOrganisationSessionProvider();

        var result = await provider.GetActiveOrganisationStateAsync(mockContext.Object);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Organisation);
        Assert.AreEqual(FakeOrganisation.Id, result.Organisation.Id);
        Assert.AreEqual(FakeOrganisation.Name, result.Organisation.Name);
    }

    #endregion
}
