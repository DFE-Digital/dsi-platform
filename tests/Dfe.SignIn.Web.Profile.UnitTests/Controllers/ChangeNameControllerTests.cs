using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangeNameControllerTests
{
    private static ChangeNameController CreateController(AutoMocker autoMocker, bool isEntra)
    {
        autoMocker.Use<IValidator<ChangeNameViewModel>>(new ChangeNameViewModelValidator());

        var controller = autoMocker.CreateInstance<ChangeNameController>();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = isEntra,
            IsInternalUser = false,
            FirstName = "Alex",
            LastName = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = autoMocker.CreateInstance<TempDataDictionary>();

        return controller;
    }

    [TestMethod]
    public async Task Index_InitialiseJobTitleInputFromUserProfile()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ChangeNameViewModel>(result);
        Assert.AreEqual("Alex", viewModel.FirstNameInput);
        Assert.AreEqual("Johnson", viewModel.LastNameInput);
    }

    [TestMethod]
    public async Task Index_PresentsExpectedView()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        var result = await controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    private static ChangeNameViewModel CreateValidChangeNameViewModel() => new() {
        FirstNameInput = "Bob",
        LastNameInput = "Clarkson",
    };

    [TestMethod]
    public async Task PostIndex_PresentsExpectedView_WhenModelIsInvalid()
    {
        var autoMocker = new AutoMocker();

        var controller = CreateController(autoMocker, isEntra: false);

        var result = await controller.PostIndex(new ChangeNameViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostIndex_FlashSuccess_WhenSuccessful()
    {
        var graphAccessToken = new GraphAccessToken { Token = "abc", ExpiresOn = DateTime.UtcNow };

        Mock<ISelectAssociatedAccountHelper> selectedAccountMock = new();
        selectedAccountMock.Setup(x => x.CreateAccessTokenForAssociatedAccount(It.IsAny<Controller>(),
        It.Is<string[]>(scopes =>
            scopes.Length == 1 &&
            scopes[0] == "https://graph.microsoft.com/.default"))).ReturnsAsync(graphAccessToken);

        var autoMocker = new AutoMocker();
        autoMocker.Use(selectedAccountMock);

        var controller = CreateController(autoMocker, isEntra: false);

        await controller.PostIndex(CreateValidChangeNameViewModel());

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Success, flashNotification.Type);
        Assert.AreEqual("Name updated successfully", flashNotification.Heading);
        Assert.AreEqual("The name associated with your account has been updated.", flashNotification.Message);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToHome_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker, isEntra: false);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public void PostCancel_FlashCancelled()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        controller.PostCancel();

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Default, flashNotification.Type);
        Assert.AreEqual("Name change cancelled", flashNotification.Heading);
        Assert.AreEqual("As you did not complete the name change process, your name change has been cancelled.", flashNotification.Message);
    }

    [TestMethod]
    public void PostCancel_RedirectsToHome()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        var result = controller.PostCancel();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task Post_InvokesEntraAndShowsSuccess()
    {
        var graphAccessToken = new GraphAccessToken { Token = "abc", ExpiresOn = DateTime.UtcNow };

        Mock<ISelectAssociatedAccountHelper> selectedAccountMock = new();
        selectedAccountMock.Setup(x => x.CreateAccessTokenForAssociatedAccount(It.IsAny<Controller>(),
        It.Is<string[]>(scopes =>
            scopes.Length == 1 &&
            scopes[0] == "https://graph.microsoft.com/.default")))
            .ReturnsAsync(graphAccessToken);

        var autoMocker = new AutoMocker();
        autoMocker.Use(selectedAccountMock);

        var controller = CreateController(autoMocker, isEntra: true);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task Post_NotInvokesEntraWhenUserAccountIsNotEntraEnabled()
    {
        Mock<ISelectAssociatedAccountHelper> selectedAccountMock = new();
        var autoMocker = new AutoMocker();
        autoMocker.Use(selectedAccountMock);

        var controller = CreateController(autoMocker, isEntra: false);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
        selectedAccountMock.Verify(
    x => x.CreateAccessTokenForAssociatedAccount(
        It.IsAny<Controller>(),
        It.IsAny<string[]>()),
    Times.Never);
    }

    [TestMethod]
    public async Task EntraAccountNameUpdateFailsCausesDSIToRollback()
    {
        var graphAccessToken = new GraphAccessToken { Token = "abc", ExpiresOn = DateTime.UtcNow };

        Mock<ISelectAssociatedAccountHelper> selectedAccountMock = new();
        Mock<IGraphApiChangeUserPersonalDetails> graphApiMock = new();
        Mock<IUsersApiClient> userClientMock = new();

        var autoMocker = new AutoMocker();

        selectedAccountMock.Setup(x => x.CreateAccessTokenForAssociatedAccount(It.IsAny<Controller>(),
       It.Is<string[]>(scopes =>
           scopes.Length == 1 &&
           scopes[0] == "https://graph.microsoft.com/.default")))
           .ReturnsAsync(graphAccessToken);

        graphApiMock
    .Setup(x => x.ChangeName(It.IsAny<Guid>(),
        "Bob",
        "Clarkson",
        graphAccessToken))
    .ThrowsAsync(new Exception("Something went wrong"));

        autoMocker.Use(selectedAccountMock);
        autoMocker.Use(userClientMock);
        autoMocker.Use(graphApiMock);

        var controller = CreateController(autoMocker, isEntra: true);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        userClientMock.Verify(
             x => x.ChangeName(It.Is<ChangeNameRequest>(request =>
                 request.FirstName == "Alex" &&
                 request.LastName == "Johnson")),
             Times.Once);
    }
}
