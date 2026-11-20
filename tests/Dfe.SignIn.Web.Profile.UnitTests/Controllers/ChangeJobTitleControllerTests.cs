using System.Net;
using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.TestHelpers.Helpers;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangeJobTitleControllerTests
{
    private static ChangeJobTitleController CreateController(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IUsersApiClient>()
            .SetReturnsDefault(Task.FromResult(RefitTestHelper.CreateSuccessResponse()));

        var controller = autoMocker.CreateInstance<ChangeJobTitleController>();

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
        var url = new Mock<IUrlHelper>();
        url.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/change-job-title");
        controller.Url = url.Object;

        return controller;
    }

    #region Index()

    [TestMethod]
    public void Index_InitialiseJobTitleInputFromUserProfile()
    {
        var controller = CreateController(new AutoMocker());

        var result = controller.Index();

        var viewModel = TypeAssert.IsViewModelType<ChangeJobTitleViewModel>(result);
        Assert.AreEqual("Software Developer", viewModel.JobTitleInput);
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

    private static ChangeJobTitleViewModel CreateValidChangeJobTitleViewModel() => new() {
        JobTitleInput = "New Job Title",
    };

    [TestMethod]
    public async Task PostIndex_PresentsExpectedView_WhenModelIsInvalid()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IValidator<ChangeJobTitleViewModel>>(new ChangeJobTitleViewModelValidator());

        autoMocker.MockValidationError<ChangeJobTitleRequest>(nameof(ChangeJobTitleRequest.NewJobTitle));

        var controller = CreateController(autoMocker);

        var result = await controller.PostIndex(new ChangeJobTitleViewModel(), CancellationToken.None);

        var viewResult = TypeAssert.IsType<ViewResult>(result);
        Assert.AreEqual("Index", viewResult.ViewName);
    }

    [TestMethod]
    public async Task PostIndex_FlashSuccess_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IValidator<ChangeJobTitleViewModel>>(new ChangeJobTitleViewModelValidator());

        var controller = CreateController(autoMocker);

        await controller.PostIndex(CreateValidChangeJobTitleViewModel(), CancellationToken.None);

        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsNotNull(flashNotification);
        Assert.AreEqual(NotificationBannerType.Success, flashNotification.Type);
        Assert.AreEqual("Job title updated successfully", flashNotification.Heading);
        Assert.AreEqual("The job title associated with your account has been updated.", flashNotification.Message);
    }

    [TestMethod]
    public async Task PostIndex_RedirectsToHome_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IValidator<ChangeJobTitleViewModel>>(new ChangeJobTitleViewModelValidator());

        var controller = CreateController(autoMocker);

        var result = await controller.PostIndex(CreateValidChangeJobTitleViewModel(), CancellationToken.None);

        var redirectResult = TypeAssert.IsType<RedirectToActionResult>(result);
        Assert.AreEqual(nameof(HomeController.Index), redirectResult.ActionName);
        Assert.AreEqual(MvcNaming.Controller<HomeController>(), redirectResult.ControllerName);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.NotFound)]
    public async Task PostIndex_DoesNotReportSuccess_WhenApiReturnsFailure(HttpStatusCode statusCode)
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IValidator<ChangeJobTitleViewModel>>(new ChangeJobTitleViewModelValidator());
        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.ChangeJobTitle(
                It.IsAny<Guid>(),
                It.IsAny<ChangeJobTitleRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RefitTestHelper.CreateProblemResponse(statusCode));

        var controller = CreateController(autoMocker);

        var result = await controller.PostIndex(CreateValidChangeJobTitleViewModel(), CancellationToken.None);

        Assert.IsFalse(result is RedirectToActionResult);
        var flashNotification = controller.TempData.GetFlashNotification();
        Assert.IsFalse(flashNotification?.Type == NotificationBannerType.Success);
        Assert.AreEqual("/Views/Error/Index.cshtml", TypeAssert.IsType<ViewResult>(result).ViewName);
    }

    [TestMethod]
    public async Task PostIndex_PresentsErrorView_WhenApiThrows()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker);
        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.ChangeJobTitle(It.IsAny<Guid>(), It.IsAny<ChangeJobTitleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var result = await controller.PostIndex(CreateValidChangeJobTitleViewModel(), CancellationToken.None);

        Assert.AreEqual("/Views/Error/Index.cshtml", TypeAssert.IsType<ViewResult>(result).ViewName);
        Assert.IsNull(controller.TempData.GetFlashNotification());
    }

    [TestMethod]
    public async Task PostIndex_PropagatesCancellation()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(autoMocker);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.ChangeJobTitle(It.IsAny<Guid>(), It.IsAny<ChangeJobTitleRequest>(), cancellation.Token))
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            controller.PostIndex(CreateValidChangeJobTitleViewModel(), cancellation.Token));
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
        Assert.AreEqual("Job title change cancelled", flashNotification.Heading);
        Assert.AreEqual("As you did not complete the job title change process, your job title change has been cancelled.", flashNotification.Message);
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
