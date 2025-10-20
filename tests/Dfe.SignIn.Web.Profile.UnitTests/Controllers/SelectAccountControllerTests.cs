using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class SelectAccountControllerTests
{
    private static SelectAccountController CreateController(AutoMocker autoMocker, bool isEntraUser)
    {
        var controller = autoMocker.CreateInstance<SelectAccountController>();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = isEntraUser,
            IsInternalUser = false,
            GivenName = "Alex",
            Surname = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return controller;
    }

    #region PostIndex(SelectAssociatedAccountViewModel)

    [TestMethod]
    public async Task SelectAssociatedAccountViewModel_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        controller.ModelState.AddModelError("", "Fake error.");

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel());

        TypeAssert.IsType<BadRequestResult>(result);
    }

    [TestMethod]
    [DataRow("/redirect-test", "/redirect-test")]
    [DataRow(null, "/")]
    public async Task SelectAssociatedAccountViewModel_Redirects_WhenNotEntraUser(string? redirectUri, string expectedRedirectUri)
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            RedirectUri = redirectUri,
        });

        var redirectResult = TypeAssert.IsType<RedirectResult>(result);
        Assert.AreEqual(expectedRedirectUri, redirectResult.Url);
    }

    [TestMethod]
    public async Task SelectAssociatedAccountViewModel_Authenticate_WhenEntraUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();

        var mockActionResult = new Mock<IActionResult>();
        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.AuthenticateAssociatedAccount(
                It.Is<Controller>(controller => controller is SelectAccountController),
                It.IsAny<string[]>(),
                It.Is<string>(redirectUri => redirectUri == "/redirect-test"),
                It.Is<bool>(force => force)
            ))
            .ReturnsAsync(mockActionResult.Object);

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            RedirectUri = "/redirect-test",
        });

        Assert.AreSame(mockActionResult.Object, result);
    }

    [TestMethod]
    public async Task SelectAssociatedAccountViewModel_Redirects_WhenEntraUserIsAlreadyAuthenticated()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.AuthenticateAssociatedAccount(
                It.Is<Controller>(controller => controller is SelectAccountController),
                It.IsAny<string[]>(),
                It.Is<string>(redirectUri => redirectUri == "/redirect-test"),
                It.Is<bool>(force => force)
            ))
            .Returns(Task.FromResult<IActionResult?>(null));

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            RedirectUri = "/redirect-test",
        });

        var redirectResult = TypeAssert.IsType<RedirectResult>(result);
        Assert.AreEqual("/redirect-test", redirectResult.Url);
    }

    #endregion
}
