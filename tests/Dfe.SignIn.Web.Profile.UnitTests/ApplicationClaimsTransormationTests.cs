using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.TestHelpers.Helpers;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests;

[TestClass]
public sealed class ApplicationClaimsTransormationTests
{

    [TestMethod]
    public async Task ReturnsPrincipleUnchangedWhenPrincipleIsNotAuthenicated()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();

        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await service.TransformAsync(principal);

        // Assert
        Assert.AreSame(principal, result);
    }

    [TestMethod]
    public async Task ReturnsPrincipleWithApproverClaimWhenUserHasApproverRights()
    {
        // Arrange
        var autoMocker = new AutoMocker();

        var userId = Guid.Parse("286101e9-a2dd-4894-bb3b-aefa8ea60ecd");

        autoMocker.GetMock<IUsersApiClient>()
            .SetReturnsDefault(Task.FromResult(RefitTestHelper.CreateSuccessResponse(new IsOrganisationApproverResponse(true))));

        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();

        var principal = new ClaimsPrincipal([
                   new ClaimsIdentity((IEnumerable<Claim>?)[
                new(ClaimTypes.NameIdentifier, userId.ToString())
            ],   authenticationType: "TestAuth")
               ]);

        // Act
        var result = await service.TransformAsync(principal);

        // Assert
        Assert.AreEqual(2, result.Claims.Count());
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString()));
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == OrganisationRole.Approver.Name));
    }

    [TestMethod]
    public async Task ReturnsPrincipleWithNoApproverClaimWhenUserHasNoApproverRights()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();
        var userId = Guid.Parse("286101e9-a2dd-4894-bb3b-aefa8ea60ecd");

        autoMocker.GetMock<IUsersApiClient>()
            .SetReturnsDefault(Task.FromResult(RefitTestHelper.CreateSuccessResponse(new IsOrganisationApproverResponse(false))));

        var principal = new ClaimsPrincipal([
            new ClaimsIdentity((IEnumerable<Claim>?)[
                new(ClaimTypes.NameIdentifier, userId.ToString())
            ],
            authenticationType: "TestAuth")
          ]);

        // Act
        var result = await service.TransformAsync(principal);

        // Assert
        Assert.AreEqual(1, result.Claims.Count());
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString()));
    }

    [TestMethod]
    public async Task ThrowsInvalidExceptionWhenUserIdClaimIsMissing()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();

        autoMocker.GetMock<IUsersApiClient>()
            .SetReturnsDefault(Task.FromResult(RefitTestHelper.CreateSuccessResponse(new IsOrganisationApproverResponse(false))));

        var principal = new ClaimsPrincipal([
            new ClaimsIdentity((IEnumerable<Claim>?)[],
            authenticationType: "TestAuth")
          ]);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransformAsync(principal));
    }

    [TestMethod]
    public async Task CallsIsApproverWithCorrectUserIdWhenUserHasDsi_User_Id_Claim()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();
        var userId = Guid.Parse("286101e9-a2dd-4894-bb3b-aefa8ea60ecd");

        autoMocker.GetMock<IUsersApiClient>()
            .SetReturnsDefault(Task.FromResult(RefitTestHelper.CreateSuccessResponse(new IsOrganisationApproverResponse(false))));

        var principal = new ClaimsPrincipal([
            new ClaimsIdentity((IEnumerable<Claim>?)[
                new(DsiClaimTypes.UserId, userId.ToString())
            ],
            authenticationType: "TestAuth")
          ]);

        // Act
        await service.TransformAsync(principal);

        // Assert
        autoMocker.GetMock<IUsersApiClient>().Verify(x => x.IsApprover(
            It.Is<Guid>(x => x == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
