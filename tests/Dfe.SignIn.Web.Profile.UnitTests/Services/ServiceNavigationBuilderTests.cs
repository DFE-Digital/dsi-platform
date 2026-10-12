using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.TestHelpers.Helpers;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.Web.Profile.UnitTests.Services;

[TestClass]
public sealed class ServiceNavigationBuilderTests
{
    [TestMethod]
    public async Task UnauthorisedUserGetsNoMenuItems()
    {
        // Arrange
        var userCtx = CreateUser(Guid.NewGuid().ToString(), authenticated: false);

        var response = new PendingApprovalCountResponse() {
            Count = 0
        };

        var userClientMock = new Mock<IUsersApiClient>();

        userClientMock.Setup(x => x.PendingApprovalCount(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefitTestHelper.CreateSuccessResponse(response));

        var options = Options.Create(CreateDefaultPlatformSettings());
        var snb = new ServiceNavigationBuilder(options, userClientMock.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public async Task StandardUserGetsReducedMenuItems()
    {
        // Arrange
        var userCtx = CreateUser(Guid.NewGuid().ToString(), authenticated: true);

        var userClientMock = new Mock<IUsersApiClient>();

        var options = Options.Create(CreateDefaultPlatformSettings());
        var snb = new ServiceNavigationBuilder(options, userClientMock.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        userClientMock.Verify(x => x.PendingApprovalCount(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.AreEqual(4, result.Length);
        Assert.IsTrue(result.Any(x => x.Text == "Services" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}my-services"));
        Assert.IsTrue(result.Any(x => x.Text == "Organisations" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}organisations"));
        Assert.IsTrue(result.Any(x => x.Text == "Profile" && x.Href.AbsoluteUri == options.Value.ProfileUrl.AbsoluteUri));
        Assert.IsTrue(result.Any(x => x.Text == "Help" && x.Href.AbsoluteUri == options.Value.HelpUrl.AbsoluteUri));
    }

    [TestMethod]
    public async Task ApproverUserGetsApprovalItemsMenuItems()
    {
        // Arrange
        var userCtx = CreateUser(Guid.NewGuid().ToString(), authenticated: true, approver: true);

        var response = new PendingApprovalCountResponse() {
            Count = 2
        };

        var userClientMock = new Mock<IUsersApiClient>();

        userClientMock.Setup(x => x.PendingApprovalCount(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefitTestHelper.CreateSuccessResponse(response));

        var options = Options.Create(CreateDefaultPlatformSettings());
        var snb = new ServiceNavigationBuilder(options, userClientMock.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        userClientMock.Verify(x => x.PendingApprovalCount(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

        Assert.AreEqual(6, result.Length);
        Assert.IsTrue(result.Any(x => x.Text == "Services" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}my-services"));
        Assert.IsTrue(result.Any(x => x.Text == "Organisations" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}organisations"));
        Assert.IsTrue(result.Any(x => x.Text == "Profile" && x.Href.AbsoluteUri == options.Value.ProfileUrl.AbsoluteUri));
        Assert.IsTrue(result.Any(x => x.Text == "Help" && x.Href.AbsoluteUri == options.Value.HelpUrl.AbsoluteUri));
        Assert.IsTrue(result.Any(x => x.Text == "Manage users" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}approvals/users"));
        Assert.IsTrue(result.Any(x => x.Text == "Requests" && x.Href.AbsoluteUri == $"{options.Value.ServicesUrl}access-requests"));

        var navigationMenuItem = (CountNavigationItemViewModel)result.Single(x => x.Text == "Requests" && x is CountNavigationItemViewModel);

        Assert.IsNotNull(navigationMenuItem);
        Assert.AreEqual(response.Count, navigationMenuItem.Count);
    }

    private static ClaimsPrincipal CreateUser(string nameId,
    bool authenticated = true,
    bool approver = false)
    {
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, nameId)
        };

        if (approver) {
            claims.Add(new Claim("Approver", string.Empty));
        }

        var identity = authenticated
            ? new ClaimsIdentity(claims, "Test")
            : new ClaimsIdentity();

        return new ClaimsPrincipal(identity);
    }

    private static PlatformSettings CreateDefaultPlatformSettings()
    {
        return new PlatformSettings {
            ServicesUrl = new Uri("https://services.test/"),
            ProfileUrl = new Uri("https://profile.test/"),
            HelpUrl = new Uri("https://help.test/"),
            SurveyUrl = new Uri("https://survey.test/"),
            ManageUrl = new Uri("https://manage.test/"),
            SupportUrl = new Uri("https://support.test/"),
            ContactUrl = new Uri("https://contact.test/"),
            CookiesUrl = new Uri("https://cookies.test/"),
            TermsUrl = new Uri("https://terms.test/"),
            PrivacyUrl = new Uri("https://privacy.test/"),
            AccessibilityUrl = new Uri("https://accessibility.test/")
        };
    }
}
