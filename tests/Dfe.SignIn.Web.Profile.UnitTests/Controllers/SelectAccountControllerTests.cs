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
        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.GetUrlFromReturnLocation(
                It.IsAny<IUrlHelper>(),
                It.Is<SelectAssociatedReturnLocation>(returnLocation
                    => returnLocation == SelectAssociatedReturnLocation.ChangePassword)
            ))
            .Returns("https://test.localhost/change-password");

        var controller = autoMocker.CreateInstance<SelectAccountController>();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = isEntraUser,
            IsInternalUser = false,
            FirstName = "Alex",
            LastName = "Johnson",
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
    public async Task SelectAssociatedAccountViewModel_Redirects_WhenNotEntraUser()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            ReturnLocation = SelectAssociatedReturnLocation.ChangePassword,
        });

        var redirectResult = TypeAssert.IsType<RedirectResult>(result);
        Assert.AreEqual("https://test.localhost/change-password", redirectResult.Url);
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
                It.Is<SelectAssociatedReturnLocation>(returnLocation
                    => returnLocation == SelectAssociatedReturnLocation.ChangePassword),
                It.Is<bool>(force => force)
            ))
            .ReturnsAsync(mockActionResult.Object);

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            ReturnLocation = SelectAssociatedReturnLocation.ChangePassword,
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
                It.Is<SelectAssociatedReturnLocation>(returnLocation
                    => returnLocation == SelectAssociatedReturnLocation.ChangePassword),
                It.Is<bool>(force => force)
            ))
            .Returns(Task.FromResult<IActionResult?>(null));

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.PostIndex(new SelectAssociatedAccountViewModel {
            ReturnLocation = SelectAssociatedReturnLocation.ChangePassword,
        });

        var redirectResult = TypeAssert.IsType<RedirectResult>(result);
        Assert.AreEqual("https://test.localhost/change-password", redirectResult.Url);
    }

    #endregion
}
