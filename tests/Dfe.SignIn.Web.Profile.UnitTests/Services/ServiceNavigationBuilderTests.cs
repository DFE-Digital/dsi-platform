using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
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
        var userCtx = CreateUser(new Guid().ToString(), authenticated: false);

        var response = new PendingApprovalCountResponse() {
            Count = 0
        };

        var interactionDispatcher = new Mock<IInteractionDispatcher>();
        interactionDispatcher.Setup(x => x.DispatchAsync(It.Is<InteractionContext<GetPendingApprovalCountRequest>>(c => true)))
            .Returns(InteractionTask.FromResult(response));

        var options = Options.Create(
            new PlatformOptions {
                ServicesUrl = new Uri("https://services.test/"),
                ProfileUrl = new Uri("https://profile.test/"),
                HelpUrl = new Uri("https://help.test/")
            });

        var snb = new ServiceNavigationBuilder(options, interactionDispatcher.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public async Task StandardUserGetsReducedMenuItems()
    {
        // Arrange
        var userCtx = CreateUser(new Guid().ToString(), authenticated: true);

        var response = new PendingApprovalCountResponse();

        var interactionDispatcher = new Mock<IInteractionDispatcher>();

        var options = Options.Create(
            new PlatformOptions {
                ServicesUrl = new Uri("https://services.test"),
                ProfileUrl = new Uri("https://profile.test"),
                HelpUrl = new Uri("https://help.test")
            });

        var snb = new ServiceNavigationBuilder(options, interactionDispatcher.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        interactionDispatcher.Verify(x => x.DispatchAsync(It.Is<InteractionContext<GetPendingApprovalCountRequest>>(c => true)), Times.Never);

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
        var userCtx = CreateUser(new Guid().ToString(), authenticated: true, approver: true);

        var response = new PendingApprovalCountResponse() {
            Count = 2
        };

        var interactionDispatcher = new Mock<IInteractionDispatcher>();
        interactionDispatcher.Setup(x => x.DispatchAsync(It.Is<InteractionContext<GetPendingApprovalCountRequest>>(c => true)))
            .Returns(InteractionTask.FromResult(response));

        var options = Options.Create(
            new PlatformOptions {
                ServicesUrl = new Uri("https://services.test"),
                ProfileUrl = new Uri("https://profile.test"),
                HelpUrl = new Uri("https://help.test")
            });

        var snb = new ServiceNavigationBuilder(options, interactionDispatcher.Object);

        // Act
        var result = await snb.Build(userCtx);

        // Assert
        interactionDispatcher.Verify(x => x.DispatchAsync(It.Is<InteractionContext<GetPendingApprovalCountRequest>>(c => true)), Times.Once);

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
}
