using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc;
using Dfe.SignIn.WebFramework.Mvc.Features;
using FluentValidation;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class ChangeJobTitleControllerTests
{
    private static ChangeJobTitleController CreateController(AutoMocker autoMocker)
    {
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
