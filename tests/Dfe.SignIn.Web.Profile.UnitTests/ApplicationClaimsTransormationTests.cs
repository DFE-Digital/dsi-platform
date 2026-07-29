using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
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

        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.IsApprover())
            .ReturnsAsync(new IsOrganisationApproverResponse(true));

        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();

        var principal = new ClaimsPrincipal([
                   new ClaimsIdentity((IEnumerable<Claim>?)[
                new(ClaimTypes.NameIdentifier, "286101e9-a2dd-4894-bb3b-aefa8ea60ecd")
            ],   authenticationType: "TestAuth")
               ]);

        // Act
        var result = await service.TransformAsync(principal);

        // Assert
        Assert.AreEqual(2, result.Claims.Count());
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "286101e9-a2dd-4894-bb3b-aefa8ea60ecd"));
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == OrganisationRoles.Approver.Name));
    }

    [TestMethod]
    public async Task ReturnsPrincipleWithNoApproverClaimWhenUserHasNoApproverRights()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<ApplicationClaimsTransformation>();

        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.IsApprover())
            .ReturnsAsync(new IsOrganisationApproverResponse(false));

        var principal = new ClaimsPrincipal([
            new ClaimsIdentity((IEnumerable<Claim>?)[
                new(ClaimTypes.NameIdentifier, "286101e9-a2dd-4894-bb3b-aefa8ea60ecd")
            ],
            authenticationType: "TestAuth")
          ]);

        // Act
        var result = await service.TransformAsync(principal);

        // Assert
        Assert.AreEqual(1, result.Claims.Count());
        Assert.AreEqual(1, result.Claims.Count(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "286101e9-a2dd-4894-bb3b-aefa8ea60ecd"));
    }
}
