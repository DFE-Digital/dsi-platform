using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.WebFramework.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class AuthControllerTests
{
    private static AuthController CreateController(AutoMocker autoMocker, HttpContext httpContext)
    {
        autoMocker.GetMock<IOptions<PlatformSettings>>()
            .Setup(x => x.Value)
            .Returns(new PlatformSettings {
                ServicesUrl = new Uri("http://services.localhost"),
                SurveyUrl = new Uri("http://survey.localhost"),
                HelpUrl = new Uri("http://help.localhost"),
                ManageUrl = new Uri("http://manage.localhost"),
                ProfileUrl = new Uri("http://profile.localhost"),
                SupportUrl = new Uri("http://support.localhost"),
                CookiesUrl = new Uri("http://cookies.localhost"),
                TermsUrl = new Uri("http://terms.localhost"),
                PrivacyUrl = new Uri("http://privacy.localhost"),
                AccessibilityUrl = new Uri("http://accessibility.localhost"),
                ContactUrl = new Uri("http://contact.localhost"),
            });

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

    #region SignOutUser()

    [TestMethod]
    public async Task SignOutUser_SignOutUser_WhenUserIsAuthenticated()
    {
        var controller = CreateControllerAuthenticated(new AutoMocker());

        var result = await controller.SignOutUser();

        var signOutResult = TypeAssert.IsType<SignOutResult>(result);
        Assert.AreEqual("http://services.localhost/signout", signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
    }

    [TestMethod]
    public async Task SignOutUser_WritesToAudit_WhenUserIsAboutToSignOut()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.GetMock<IAuditWriter>()
            .Setup(x => x.Log(It.IsAny<WriteToAuditRequest>()))
            .Callback<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.SignOutUser();

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.SignOut, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("User signing out", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedWriteToAudit.UserId);
    }

    #endregion

    [TestMethod]
    [DataRow(nameof(AuthController.SignOutUser), "http://services.localhost/signout")]
    [DataRow(nameof(AuthController.EndSessionTimeout), "/session-timeout")]
    [DataRow(nameof(AuthController.Timeout), "http://services.localhost/signout")]
    public async Task SignOut_ReturnsResultAndLogsFailure_WhenAuditThrows(string action, string redirectUri)
    {
        var autoMocker = new AutoMocker();
        var exception = new InvalidOperationException("Audit unavailable");
        autoMocker.GetMock<IAuditWriter>()
            .Setup(x => x.Log(It.IsAny<WriteToAuditRequest>()))
            .ThrowsAsync(exception);
        var controller = CreateControllerAuthenticated(autoMocker);

        var result = action switch {
            nameof(AuthController.SignOutUser) => await controller.SignOutUser(),
            nameof(AuthController.EndSessionTimeout) => await controller.EndSessionTimeout(),
            nameof(AuthController.Timeout) => await controller.Timeout(),
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

        var signOutResult = TypeAssert.IsType<SignOutResult>(result);
        Assert.AreEqual(redirectUri, signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(ExternalAuthConstants.CookiesSchemeName, signOutResult.AuthenticationSchemes);
        autoMocker.GetMock<ILogger<AuthController>>()
            .Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [TestMethod]
    public async Task SignOutUser_SkipsAudit_WhenUserIsAnonymous()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateControllerAnonymous(autoMocker);

        var result = await controller.SignOutUser();

        Assert.AreEqual("http://services.localhost/", TypeAssert.IsType<SignOutResult>(result).Properties?.RedirectUri);
        autoMocker.GetMock<IAuditWriter>()
            .Verify(x => x.Log(It.IsAny<WriteToAuditRequest>()), Times.Never);
    }

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
        autoMocker.GetMock<IAuditWriter>()
            .Setup(x => x.Log(It.IsAny<WriteToAuditRequest>()))
            .Callback<WriteToAuditRequest>(req => capturedWriteToAudit = req);

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
        Assert.AreEqual("http://services.localhost/signout", signOutResult.Properties?.RedirectUri);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
    }

    [TestMethod]
    public async Task Timeout_WritesToAudit_WhenUserIsAboutToSignOut()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.GetMock<IAuditWriter>()
            .Setup(x => x.Log(It.IsAny<WriteToAuditRequest>()))
            .Callback<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var controller = CreateControllerAuthenticated(autoMocker);

        await controller.Timeout();

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.SignOut, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("User signing out", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedWriteToAudit.UserId);
    }

    #endregion
}
