using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class OrganisationClaimManagerTests
{
    private static Mock<HttpContext> SetupFakeContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        mockContext.Setup(mock => mock.RequestServices)
            .Returns(autoMocker);

        return mockContext;
    }

    private static ClaimsPrincipal SetupFakeUser(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity());
        mockContext.Setup(x => x.User)
            .Returns(fakeUser);

        return fakeUser;
    }

    #region UpdateOrganisationClaim(HttpContext, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task UpdateOrganisationClaim_Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(null!, "null");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task UpdateOrganisationClaim_Throws_WhenOrganisationJsonArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupFakeContext(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, null!);
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_AddsOrganisationClaim()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupFakeContext(autoMocker);
        SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        manager.SignInProxyAsync = (context, principal) => {
            capturedPrincipal = principal;
            return Task.CompletedTask;
        };

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, "1");

        Assert.IsNotNull(capturedPrincipal);
        Assert.AreEqual(1, capturedPrincipal.Claims.Count());
        Assert.IsTrue(
            capturedPrincipal.HasClaim(DsiClaimTypes.Organisation, "1")
        );
    }

    [TestMethod]
    public async Task UpdateOrganisationClaim_ReplacesAnyExistingOrganisationClaims()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupFakeContext(autoMocker);
        var fakeUser = SetupFakeUser(autoMocker);

        var manager = autoMocker.CreateInstance<OrganisationClaimManager>();

        ClaimsPrincipal? capturedPrincipal = null;
        manager.SignInProxyAsync = (context, principal) => {
            capturedPrincipal = principal;
            return Task.CompletedTask;
        };

        fakeUser.AddIdentity(new ClaimsIdentity([
            new Claim(DsiClaimTypes.Organisation, "1", JsonClaimValueTypes.Json),
            new Claim(DsiClaimTypes.Organisation, "2", JsonClaimValueTypes.Json),
        ]));

        await manager.UpdateOrganisationClaimAsync(mockContext.Object, "3");

        Assert.IsNotNull(capturedPrincipal);
        Assert.AreEqual(1, capturedPrincipal.Claims.Count());
        Assert.IsTrue(
            capturedPrincipal.HasClaim(DsiClaimTypes.Organisation, "3")
        );
    }

    #endregion
}
