using System.Net;
using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Moq.AutoMock;
using Refit;

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

    private static ChangeNameViewModel CreateValidChangeNameViewModel() => new() {
        FirstNameInput = "Bob",
        LastNameInput = "Clarkson",
    };

    [TestMethod]
    public void Index_InitialiseJobTitleInputFromUserProfile()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        var result = controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ChangeNameViewModel>(result);
        Assert.AreEqual("Alex", viewModel.FirstNameInput);
        Assert.AreEqual("Johnson", viewModel.LastNameInput);
    }

    [TestMethod]
    public void Index_PresentsExpectedView()
    {
        var controller = CreateController(new AutoMocker(), isEntra: false);

        var result = controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

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
        var autoMocker = new AutoMocker();

        var userClientMock = new Mock<IUsersApiClient>();
        userClientMock.Setup(x => x.ChangeName(It.IsAny<Guid>(), It.IsAny<ChangeNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(new HttpResponseMessage(HttpStatusCode.OK), null, new RefitSettings()));

        autoMocker.Use(userClientMock.Object);

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

        var userClientMock = new Mock<IUsersApiClient>();
        userClientMock.Setup(x => x.ChangeName(It.IsAny<Guid>(), It.IsAny<ChangeNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(new HttpResponseMessage(HttpStatusCode.OK), null, new RefitSettings()));

        autoMocker.Use(userClientMock.Object);
        var controller = CreateController(autoMocker, isEntra: false);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task PostIndex_ShowsErrorMessageAndReRendersView_WhenApiClientReturnsError()
    {
        var autoMocker = new AutoMocker();

        var userClientMock = new Mock<IUsersApiClient>();
        userClientMock.Setup(x => x.ChangeName(It.IsAny<Guid>(), It.IsAny<ChangeNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<object>(new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings()));

        autoMocker.Use(userClientMock.Object);
        var controller = CreateController(autoMocker, isEntra: true);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
        Assert.AreEqual("We couldn't save your name right now. Please try again.", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task PostIndex_ShowsErrorMessageAndReRendersView_WhenApiClientThrows()
    {
        var autoMocker = new AutoMocker();

        var userClientMock = new Mock<IUsersApiClient>();
        userClientMock.Setup(x => x.ChangeName(It.IsAny<Guid>(), It.IsAny<ChangeNameRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network failure"));

        autoMocker.Use(userClientMock.Object);
        var controller = CreateController(autoMocker, isEntra: true);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
        Assert.AreEqual("We couldn't save your name right now. Please try again.", controller.ModelState[string.Empty].Errors[0].ErrorMessage);
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
}
