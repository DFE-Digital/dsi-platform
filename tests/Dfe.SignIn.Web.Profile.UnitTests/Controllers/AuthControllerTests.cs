using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class AuthControllerTests
{
    private static AuthController CreateController(AutoMocker autoMocker, HttpContext httpContext)
    {
        var controller = autoMocker.CreateInstance<AuthController>();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(
                It.Is<UrlActionContext>(context => context.Action == nameof(AuthController.Timeout))
            ))
            .Returns("/session-timeout");
        controller.Url = mockUrlHelper.Object;

        return controller;
    }

    private static AuthController CreateControllerAuthenticated(AutoMocker autoMocker)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = false,
            IsInternalUser = false,
            FirstName = "Alex",
            LastName = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ], "TestAuth"));

        return CreateController(autoMocker, httpContext);
    }

    private static AuthController CreateControllerAnonymous(AutoMocker autoMocker)
    {
        return CreateController(autoMocker, new DefaultHttpContext());
    }

    #region SignOut()

    [TestMethod]
    public async Task SignOut_SignOutUser_WhenUserIsAuthenticated()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.SignOut();

        var signOutResult = TypeAssert.IsType<SignOutResult>(result);
        Assert.IsNull(signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
    }

    [TestMethod]
    public async Task SignOut_WritesToAudit_WhenUserIsAboutToSignOut()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.SignOut();

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.SignOut, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("User signing out", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedWriteToAudit.UserId);
    }

    #endregion

    #region EndSessionTimeout()

    [TestMethod]
    public async Task EndSessionTimeout_SignOutUser_WhenUserIsAuthenticated()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.EndSessionTimeout();

        var signOutResult = TypeAssert.IsType<SignOutResult>(result);
        Assert.AreEqual("/session-timeout", signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
    }

    [TestMethod]
    public async Task EndSessionTimeout_WritesToAudit_WhenUserIsAboutToSignOut()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.EndSessionTimeout();

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.SignOut, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("User signing out", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedWriteToAudit.UserId);
    }

    #endregion

    #region Timeout()

    [TestMethod]
    public async Task Timeout_PresentsSessionTimeoutView()
    {
        var controller = CreateControllerAnonymous(new AutoMocker());

        var result = await controller.Timeout();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Auth/SessionTimeout", viewResult.ViewName);
    }

    [TestMethod]
    public async Task Timeout_SignOutUser_WhenUserIsAuthenticated()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.Timeout();

        var signOutResult = TypeAssert.IsType<SignOutResult>(result);
        Assert.AreEqual("/", signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
    }

    [TestMethod]
    public async Task Timeout_WritesToAudit_WhenUserIsAboutToSignOut()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.Timeout();

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.SignOut, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("User signing out", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedWriteToAudit.UserId);
    }

    #endregion
}
