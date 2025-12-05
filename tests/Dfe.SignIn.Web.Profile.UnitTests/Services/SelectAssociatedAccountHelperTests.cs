using System.Security.Claims;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Identity.Web;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Services;

[TestClass]
public sealed class SelectAssociatedAccountHelperTests
{
    private sealed class FakeController : Controller { }

    private static IUrlHelper CreateMockUrlHelper()
    {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(
                It.Is<UrlActionContext>(ctx =>
                    ctx.Action == nameof(HomeController.Index) &&
                    ctx.Controller == MvcNaming.Controller<HomeController>()
                )
            ))
            .Returns("https://test.localhost/");
        mockUrlHelper
            .Setup(x => x.Action(
                It.Is<UrlActionContext>(ctx =>
                    ctx.Action == nameof(ChangePasswordController.Index) &&
                    ctx.Controller == MvcNaming.Controller<ChangePasswordController>()
                )
            ))
            .Returns("https://test.localhost/change-password");

        return mockUrlHelper.Object;
    }

    private static FakeController CreateMockControllerWithNonEntraUser(AutoMocker autoMocker)
    {
        var controller = new FakeController();

        var httpContext = new DefaultHttpContext {
            RequestServices = autoMocker,
        };

        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = false,
            IsInternalUser = false,
            GivenName = "Alex",
            Surname = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.Url = CreateMockUrlHelper();

        return controller;
    }

    private static FakeController CreateMockControllerWithEntraUser(AutoMocker autoMocker)
    {
        autoMocker.GetMock<ITokenAcquisition>()
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "fake-scope"),
                It.Is<string>(authenticationScheme => authenticationScheme == ExternalAuthConstants.OpenIdConnectSchemeName),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()
            ))
            .ReturnsAsync("FakeToken1234");

        var controller = new FakeController();

        var httpContext = new DefaultHttpContext {
            RequestServices = autoMocker,
        };

        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = true,
            IsInternalUser = false,
            GivenName = "Alex",
            Surname = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.Url = CreateMockUrlHelper();

        return controller;
    }

    #region GetUrlFromReturnLocation(IUrlHelper, SelectAssociatedReturnLocation)

    [TestMethod]
    public void GetUrlFromReturnLocation_Throws_WhenUrlHelperArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => service.GetUrlFromReturnLocation(
                urlHelper: null!,
                returnLocation: SelectAssociatedReturnLocation.Home
            ));
    }

    #endregion

    #region AuthenticateAssociatedAccount(Controller, string[], string?, bool)

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_Throws_WhenControllerArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => service.AuthenticateAssociatedAccount(
                controller: null!,
                scopes: [],
                returnLocation: SelectAssociatedReturnLocation.Home,
                force: false
            ));
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_Throws_WhenScopesArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => service.AuthenticateAssociatedAccount(
                controller: new Mock<Controller>().Object,
                scopes: null!,
                returnLocation: SelectAssociatedReturnLocation.Home,
                force: false
            ));
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_ChallengeUser_WhenFailedToAuthenticate()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Fail("Failed."));

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        var result = await service.AuthenticateAssociatedAccount(
            controller, ["fake-scope"], SelectAssociatedReturnLocation.ChangePassword);

        TypeAssert.IsType<ChallengeResult>(result);
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_ChallengeUser_WhenForced()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal();

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        var result = await service.AuthenticateAssociatedAccount(
            controller, ["fake-scope"], SelectAssociatedReturnLocation.ChangePassword, force: true);

        TypeAssert.IsType<ChallengeResult>(result);
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_ChallengeUser_WhenDsiUserIdClaimMissing()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal();

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        var result = await service.AuthenticateAssociatedAccount(
            controller, ["fake-scope"], SelectAssociatedReturnLocation.ChangePassword, force: false);

        TypeAssert.IsType<ChallengeResult>(result);
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_PresentsSelectAssociatedAccountView_WhenAccountMismatch()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "c59d1905-3a44-4e2b-8a32-3859a13c0c4f"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        var result = await service.AuthenticateAssociatedAccount(controller, ["fake-scope"],
            SelectAssociatedReturnLocation.ChangePassword, force: false);

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("SelectAssociatedAccount", viewResult.ViewName);

        var viewModel = TypeAssert.IsViewModelType<SelectAssociatedAccountViewModel>(result!);
        Assert.AreEqual(SelectAssociatedReturnLocation.ChangePassword, viewModel.ReturnLocation);
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_AttemptsToGetAccessToken()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        await service.AuthenticateAssociatedAccount(controller, ["fake-scope"],
            SelectAssociatedReturnLocation.ChangePassword, force: false);

        autoMocker.Verify<ITokenAcquisition>(
            x => x.GetAccessTokenForUserAsync(
                It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "fake-scope"),
                It.Is<string>(authenticationScheme => authenticationScheme == ExternalAuthConstants.OpenIdConnectSchemeName),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_ReturnsNull_WhenCreatedAccessToken()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        var result = await service.AuthenticateAssociatedAccount(
            controller, ["fake-scope"], SelectAssociatedReturnLocation.ChangePassword, force: false);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task AuthenticateAssociatedAccount_ChallengeUser_WhenUnableToCreateAccessToken()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(
                new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName))
            );

        var controller = CreateMockControllerWithEntraUser(autoMocker);
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        autoMocker.GetMock<ITokenAcquisition>()
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.Is<string[]>(scopes => scopes.Length == 1 && scopes[0] == "fake-scope"),
                It.Is<string>(authenticationScheme => authenticationScheme == ExternalAuthConstants.OpenIdConnectSchemeName),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()
            ))
            .Throws(new InvalidOperationException());

        var result = await service.AuthenticateAssociatedAccount(
            controller, ["fake-scope"], SelectAssociatedReturnLocation.ChangePassword, force: false);

        TypeAssert.IsType<ChallengeResult>(result);
    }

    #endregion

    #region CreateAccessTokenForAssociatedAccount(Controller, string[])

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_Throws_WhenControllerArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => service.CreateAccessTokenForAssociatedAccount(
                controller: null!,
                scopes: []
            ));
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_Throws_WhenScopesArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => service.CreateAccessTokenForAssociatedAccount(
                controller: new Mock<Controller>().Object,
                scopes: null!
            ));
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_ReturnsNull_WhenNotEntraUser()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();
        var controller = CreateMockControllerWithNonEntraUser(autoMocker);

        var result = await service.CreateAccessTokenForAssociatedAccount(controller, ["fake-scope"]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_Throws_WhenNotAuthenticatedWithAssociatedAccount()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Fail("Failed."));

        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();
        var controller = CreateMockControllerWithEntraUser(autoMocker);

        var exception = await Assert.ThrowsExactlyAsync<BadHttpRequestException>(()
            => service.CreateAccessTokenForAssociatedAccount(controller, ["fake-scope"]));

        Assert.AreEqual("Not authenticated with associated account.", exception.Message);
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_Throws_WhenMissingDsiUserIdClaim()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal();

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName)));

        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();
        var controller = CreateMockControllerWithEntraUser(autoMocker);

        var exception = await Assert.ThrowsExactlyAsync<BadHttpRequestException>(()
            => service.CreateAccessTokenForAssociatedAccount(controller, ["fake-scope"]));

        Assert.AreEqual("Missing required claim 'dsi_user_id'.", exception.Message);
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_Throws_WhenDsiUserIdMismatch()
    {
        var autoMocker = new AutoMocker();

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "ffa39281-b388-4edc-998e-afb58a63f28a"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName)));

        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();
        var controller = CreateMockControllerWithEntraUser(autoMocker);

        var exception = await Assert.ThrowsExactlyAsync<BadHttpRequestException>(()
            => service.CreateAccessTokenForAssociatedAccount(controller, ["fake-scope"]));

        Assert.AreEqual("Account mismatch.", exception.Message);
    }

    [TestMethod]
    public async Task CreateAccessTokenForAssociatedAccount_ReturnsAcquiredToken()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<TimeProvider>(new MockTimeProvider(new DateTimeOffset(2025, 11, 19, 14, 11, 43, TimeSpan.Zero)));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity([
            new("dsi_user_id", "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ]));

        autoMocker.GetMock<IAuthenticationService>()
            .Setup(x => x.AuthenticateAsync(
                It.IsAny<HttpContext>(),
                It.Is<string>(scheme => scheme == ExternalAuthConstants.OpenIdConnectSchemeName)
            ))
            .ReturnsAsync(AuthenticateResult.Success(new(fakeUser, ExternalAuthConstants.OpenIdConnectSchemeName)));

        var service = autoMocker.CreateInstance<SelectAssociatedAccountHelper>();
        var controller = CreateMockControllerWithEntraUser(autoMocker);

        var accessToken = await service.CreateAccessTokenForAssociatedAccount(controller, ["fake-scope"]);

        Assert.IsNotNull(accessToken);
        Assert.AreEqual("FakeToken1234", accessToken.Token);
        Assert.AreEqual(new DateTimeOffset(2025, 11, 19, 14, 12, 43, TimeSpan.Zero), accessToken.ExpiresOn);
    }

    #endregion
}
