using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Graph;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangePasswordControllerTests
{
    private static ChangePasswordController CreateController(AutoMocker autoMocker, bool isEntraUser)
    {
        var controller = autoMocker.CreateInstance<ChangePasswordController>();

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

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, "15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
        ], "TestAuth"));

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = autoMocker.CreateInstance<TempDataDictionary>();
        controller.Url = new Mock<IUrlHelper>().Object;

        return controller;
    }

    #region Index()

    [TestMethod]
    public async Task Index_ClearsAnyPreviousInputs()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        controller.ModelState.SetModelValue(nameof(ChangePasswordViewModel.CurrentPasswordInput), "abc123", "abc123");
        controller.ModelState.SetModelValue(nameof(ChangePasswordViewModel.NewPasswordInput), "abc123", "abc123");
        controller.ModelState.SetModelValue(nameof(ChangePasswordViewModel.ConfirmNewPasswordInput), "abc123", "abc123");

        await controller.Index();

        var currentPasswordInputState = controller.ModelState[nameof(ChangePasswordViewModel.CurrentPasswordInput)];
        Assert.IsNull(currentPasswordInputState!.RawValue);
        Assert.AreEqual("", currentPasswordInputState!.AttemptedValue);

        var currentNewPasswordInputState = controller.ModelState[nameof(ChangePasswordViewModel.NewPasswordInput)];
        Assert.IsNull(currentNewPasswordInputState!.RawValue);
        Assert.AreEqual("", currentNewPasswordInputState!.AttemptedValue);

        var currentConfirmNewPasswordInputState = controller.ModelState[nameof(ChangePasswordViewModel.ConfirmNewPasswordInput)];
        Assert.IsNull(currentConfirmNewPasswordInputState!.RawValue);
        Assert.AreEqual("", currentConfirmNewPasswordInputState!.AttemptedValue);
    }

    [TestMethod]
    public async Task Index_PresentsExpectedView()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        var result = await controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task Index_RedirectsToAuthenticate_WhenNonAuthenticatedEntraUser()
    {
        var autoMocker = new AutoMocker();

        var mockActionResult = new Mock<IActionResult>();
        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.AuthenticateAssociatedAccount(
                It.Is<Controller>(controller => controller is ChangePasswordController),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.Is<bool>(force => !force)
            ))
            .ReturnsAsync(mockActionResult.Object);

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.Index();

        Assert.AreSame(mockActionResult.Object, result);
    }

    [TestMethod]
    public async Task Index_PresentsExpectedView_WhenAuthenticatedEntraUser()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.AuthenticateAssociatedAccount(
                It.Is<Controller>(controller => controller is ChangePasswordController),
                It.IsAny<string[]>(),
                It.IsAny<string>(),
                It.Is<bool>(force => !force)
            ))
            .Returns(Task.FromResult<IActionResult?>(null));

        var controller = CreateController(autoMocker, isEntraUser: true);

        var result = await controller.Index();

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    #endregion

    #region PostIndex()

    private static ChangePasswordViewModel CreateValidChangePasswordViewModel() => new() {
        CurrentPasswordInput = "oldpassword",
        NewPasswordInput = "newpassword",
        ConfirmNewPasswordInput = "newpassword",
    };

    [TestMethod]
    public async Task PostIndex_PresentsExpectedView_WhenModelIsInvalid()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockValidationError<SelfChangePasswordRequest>(nameof(SelfChangePasswordRequest.CurrentPassword));

        var controller = CreateController(autoMocker, isEntraUser: false);

        var result = await controller.PostIndex(new ChangePasswordViewModel());

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostIndex_DispatchesExpectedInteraction()
    {
        var autoMocker = new AutoMocker();

        SelfChangePasswordRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SelfChangePasswordRequest>(r => capturedRequest = r);

        var controller = CreateController(autoMocker, isEntraUser: false);

        await controller.PostIndex(CreateValidChangePasswordViewModel());

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedRequest.UserId);
        Assert.IsNull(capturedRequest.GraphAccessToken);
        Assert.AreEqual("oldpassword", capturedRequest.CurrentPassword);
        Assert.AreEqual("newpassword", capturedRequest.NewPassword);
        Assert.AreEqual("newpassword", capturedRequest.ConfirmNewPassword);
    }

    [TestMethod]
    public async Task PostIndex_DispatchesExpectedInteraction_WhenEntraUser()
    {
        var autoMocker = new AutoMocker();

        var fakeAccessToken = new GraphAccessToken {
            Token = "fake-token",
            ExpiresOn = new DateTimeOffset(2025, 11, 05, 1, 23, 20, TimeSpan.Zero),
        };

        autoMocker.GetMock<ISelectAssociatedAccountHelper>()
            .Setup(x => x.CreateAccessTokenForAssociatedAccount(
                It.Is<Controller>(controller => controller is ChangePasswordController),
                It.IsAny<string[]>()
            ))
            .ReturnsAsync(fakeAccessToken);

        SelfChangePasswordRequest? capturedRequest = null;
        autoMocker.CaptureRequest<SelfChangePasswordRequest>(r => capturedRequest = r);

        var controller = CreateController(autoMocker, isEntraUser: true);

        await controller.PostIndex(CreateValidChangePasswordViewModel());

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"), capturedRequest.UserId);
        Assert.AreSame(fakeAccessToken, capturedRequest.GraphAccessToken);
        Assert.AreEqual("oldpassword", capturedRequest.CurrentPassword);
        Assert.AreEqual("newpassword", capturedRequest.NewPassword);
        Assert.AreEqual("newpassword", capturedRequest.ConfirmNewPassword);
    }

    [TestMethod]
    public async Task PostIndex_FlashSuccess_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker, isEntraUser: false);

        await controller.PostIndex(CreateValidChangePasswordViewModel());

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Success, flashNotification.Type);
        Assert.AreEqual("Password changed successfully", flashNotification.Heading);
        Assert.AreEqual("The password associated with your account has been updated.", flashNotification.Message);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToHome_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker, isEntraUser: false);

        var result = await controller.PostIndex(CreateValidChangePasswordViewModel());

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion

    #region PostCancel()

    [TestMethod]
    public void PostCancel_FlashCancelled()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        controller.PostCancel();

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Default, flashNotification.Type);
        Assert.AreEqual("Password change cancelled", flashNotification.Heading);
        Assert.AreEqual("As you did not complete the password change process, your password change has been cancelled.", flashNotification.Message);
    }

    [TestMethod]
    public void PostCancel_RedirectsToHome()
    {
        var controller = CreateController(new AutoMocker(), isEntraUser: false);

        var result = controller.PostCancel();

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    #endregion
}
