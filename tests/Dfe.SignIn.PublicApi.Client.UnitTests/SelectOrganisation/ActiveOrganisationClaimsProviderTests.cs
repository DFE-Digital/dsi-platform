using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.Internal;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class ActiveOrganisationClaimsProviderTests
{
    private const string FakeUserId = "6c2afda0-62ce-4f88-9a23-a252bc647856";
    private const string FakeSessionId = "92c37f9d-253b-42cd-95eb-ddeb9d1e03b8";

    private const string FakeOrganisation1ClaimValue = /*lang=json,strict*/ """
        { "id": "6ab8e6c9-f953-42e4-be0d-4a7ace4b229d" }
    """;

    private static readonly OrganisationDetails FakeOrganisation1 = new() {
        Id = new Guid("6ab8e6c9-f953-42e4-be0d-4a7ace4b229d"),
    };

    private const string FakeOrganisation2ClaimValue = /*lang=json,strict*/ """
        { "id": "7d6f1125-dd53-4d61-8ae6-3e20d5091b1c" }
    """;

    private static readonly OrganisationDetails FakeOrganisation3 = new() {
        Id = new Guid("7122a5eb-a66c-49ca-a3cc-73c123680bbd"),
    };

    private static void SetupFakeOptions(
        AutoMocker autoMocker,
        AuthenticationOrganisationSelectorOptions? options = null)
    {
        autoMocker.GetMock<IOptions<AuthenticationOrganisationSelectorOptions>>()
            .Setup(mock => mock.Value)
            .Returns(options ?? new AuthenticationOrganisationSelectorOptions());
    }

    private static Mock<IHttpContext> SetupFakeContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        return mockContext;
    }

    private static ClaimsPrincipal SetupFakeUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(DsiClaimTypes.UserId, FakeUserId),
            new Claim(DsiClaimTypes.SessionId, FakeSessionId),
        ]));

        mockContext.Setup(x => x.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    #region SetActiveOrganisationAsync(HttpContext, OrganisationDetails?)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task SetActiveOrganisationAsync_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        await instance.SetActiveOrganisationAsync(null!, null);
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_RetainsPrimaryIdentity()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await instance.SetActiveOrganisationAsync(mockContext.Object, FakeOrganisation1);

        var primaryIdentity = capturedPrincipal?.GetPrimaryIdentity();
        Assert.IsNotNull(primaryIdentity);
        Assert.IsTrue(
            primaryIdentity.HasClaim(DsiClaimTypes.SessionId, FakeSessionId)
        );
        Assert.IsTrue(
            primaryIdentity.HasClaim(DsiClaimTypes.UserId, FakeUserId)
        );
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_AddsSessionIdClaim()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await instance.SetActiveOrganisationAsync(mockContext.Object, FakeOrganisation1);

        var dsiIdentity = capturedPrincipal?.GetDsiOrganisationIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.SessionId, FakeSessionId)
        );
    }

    public static IEnumerable<object[]> GetDynamicData_SetActiveOrganisationAsync_AddsOrganisationClaim()
    {
        return [
            [null!, "null"],
            [FakeOrganisation1, DsiClaimExtensions.SerializeDsiOrganisation(FakeOrganisation1)],
        ];
    }

    [DynamicData("GetDynamicData_SetActiveOrganisationAsync_AddsOrganisationClaim")]
    [DataTestMethod]
    public async Task SetActiveOrganisationAsync_AddsOrganisationClaim(OrganisationDetails? organisation, string expectedClaimValue)
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await instance.SetActiveOrganisationAsync(mockContext.Object, organisation);

        var dsiIdentity = capturedPrincipal?.GetDsiOrganisationIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.Organisation, expectedClaimValue)
        );
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_AddsOrganisationClaim_WhenOrganisationArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await instance.SetActiveOrganisationAsync(mockContext.Object, null);

        var dsiIdentity = capturedPrincipal?.GetDsiOrganisationIdentity();
        Assert.IsNotNull(dsiIdentity);
        Assert.IsTrue(
            dsiIdentity.HasClaim(DsiClaimTypes.Organisation, "null")
        );
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_ReplacesAnyExistingOrganisationClaims()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        var fakeUser = SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        fakeUser.AddIdentity(new ClaimsIdentity([
            new Claim(DsiClaimTypes.Organisation, FakeOrganisation1ClaimValue, JsonClaimValueTypes.Json),
            new Claim(DsiClaimTypes.Organisation, FakeOrganisation2ClaimValue, JsonClaimValueTypes.Json),
        ], PublicApiConstants.AuthenticationType));

        ClaimsPrincipal? capturedPrincipal = null;
        mockContext
            .Setup(mock => mock.SignInAsync(It.IsAny<ClaimsPrincipal>()))
            .Callback<ClaimsPrincipal>(principal => capturedPrincipal = principal);

        await instance.SetActiveOrganisationAsync(mockContext.Object, FakeOrganisation3);

        Assert.IsNotNull(capturedPrincipal);
        var dfeSignInIdentities = capturedPrincipal.Identities
            .Where(identity => identity.AuthenticationType == PublicApiConstants.AuthenticationType);
        Assert.AreEqual(1, dfeSignInIdentities.Count());
        Assert.AreEqual(1, capturedPrincipal.FindAll(DsiClaimTypes.Organisation).Count());
        Assert.AreEqual(FakeOrganisation3.Id, capturedPrincipal.GetDsiOrganisation<OrganisationDetails>()!.Id);
    }

    [TestMethod]
    public async Task SetActiveOrganisationAsync_DoesNotFetchRolesFromPublicApi_WhenNoFlagsSpecified()
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var instance = autoMocker.CreateInstance<ActiveOrganisationClaimsProvider>();

        await instance.SetActiveOrganisationAsync(mockContext.Object, FakeOrganisation1);

        autoMocker.Verify<IInteractor<GetUserAccessToServiceRequest, GetUserAccessToServiceResponse>>(
            interactor => interactor.InvokeAsync(
                It.IsAny<GetUserAccessToServiceRequest>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never);
    }

    #endregion
}
