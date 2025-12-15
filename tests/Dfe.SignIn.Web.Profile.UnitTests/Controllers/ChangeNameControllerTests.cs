using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangeNameControllerTests
{
    private static ChangeNameController CreateController(AutoMocker autoMocker)
    {
        var controller = autoMocker.CreateInstance<ChangeNameController>();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
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

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = autoMocker.CreateInstance<TempDataDictionary>();

        return controller;
    }

    #region Index()

    [TestMethod]
    public void Index_InitialiseJobTitleInputFromUserProfile()
    {
        var controller = CreateController(new AutoMocker());

        var result = controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ChangeNameViewModel>(result);
        Assert.AreEqual("Alex", viewModel.FirstNameInput);
        Assert.AreEqual("Johnson", viewModel.LastNameInput);
    }

    [TestMethod]
    public void Index_PresentsExpectedView()
    {
        var controller = CreateController(new AutoMocker());

        var result = controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    #endregion

    #region PostIndex()

    private static ChangeNameViewModel CreateValidChangeNameViewModel() => new() {
        FirstNameInput = "Bob",
        LastNameInput = "Clarkson",
    };

    [TestMethod]
    public async Task PostIndex_PresentsExpectedView_WhenModelIsInvalid()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockValidationError<ChangeNameRequest>(nameof(ChangeNameRequest.FirstName));

        var controller = CreateController(autoMocker);

        var result = await controller.PostIndex(new ChangeNameViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostIndex_DispatchesExpectedInteraction()
    {
        var autoMocker = new AutoMocker();

        ChangeNameRequest? capturedRequest = null;
        autoMocker.CaptureRequest<ChangeNameRequest>(r => capturedRequest = r);

        var controller = CreateController(autoMocker);

        await controller.PostIndex(CreateValidChangeNameViewModel());

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedRequest.UserId);
        Assert.AreEqual("Bob", capturedRequest.FirstName);
        Assert.AreEqual("Clarkson", capturedRequest.LastName);
    }

    [TestMethod]
    public async Task PostIndex_FlashSuccess_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker);

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
        var controller = CreateController(autoMocker);

        var result = await controller.PostIndex(CreateValidChangeNameViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion

    #region PostCancel()

    [TestMethod]
    public void PostCancel_FlashCancelled()
    {
        var controller = CreateController(new AutoMocker());

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
        var controller = CreateController(new AutoMocker());

        var result = controller.PostCancel();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion
}
