using System.Text.Json;
using Dfe.SignIn.WebFramework.Models;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class FlashNotificationExtensionsTests
{
    #region SetFlashNotification(Controller, string, string?)

    [TestMethod]
    public void SetFlashNotification_Throws_WhenControllerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => FlashNotificationExtensions.SetFlashNotification(null!, "Example message"));
    }

    [TestMethod]
    public void SetFlashNotification_Throws_WhenMessageArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => FlashNotificationExtensions.SetFlashNotification(mockController.Object, null!));
    }

    [TestMethod]
    public void SetFlashNotification_Throws_WhenMessageArgumentIsEmpty()
    {
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentException>(()
            => FlashNotificationExtensions.SetFlashNotification(mockController.Object, ""));
    }

    [TestMethod]
    public void SetFlashNotification_SetsExpectedTempData()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();
        mockController.Object.TempData = autoMocker.CreateInstance<TempDataDictionary>();

        FlashNotificationExtensions.SetFlashNotification(mockController.Object, "Example flash message.", "Example heading");

        string? capturedFlashNotification = mockController.Object.TempData[FlashNotificationExtensions.TempDataKey] as string;
        Assert.IsNotNull(capturedFlashNotification);

        var viewModel = JsonSerializer.Deserialize<FlashNotificationViewModel>(capturedFlashNotification);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(NotificationBannerType.Default, viewModel.Type);
        Assert.AreEqual("Example heading", viewModel.Heading);
        Assert.AreEqual("Example flash message.", viewModel.Message);
    }

    #endregion

    #region SetFlashSuccess(Controller, string, string?)

    [TestMethod]
    public void SetFlashSuccess_Throws_WhenControllerArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => FlashNotificationExtensions.SetFlashSuccess(null!, "Example message"));
    }

    [TestMethod]
    public void SetFlashSuccess_Throws_WhenMessageArgumentIsNull()
    {
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => FlashNotificationExtensions.SetFlashSuccess(mockController.Object, null!));
    }

    [TestMethod]
    public void SetFlashSuccess_Throws_WhenMessageArgumentIsEmpty()
    {
        var mockController = new Mock<Controller>();

        Assert.ThrowsExactly<ArgumentException>(()
            => FlashNotificationExtensions.SetFlashSuccess(mockController.Object, ""));
    }

    [TestMethod]
    public void SetFlashSuccess_SetsExpectedTempData()
    {
        var autoMocker = new AutoMocker();
        var mockController = new Mock<Controller>();
        mockController.Object.TempData = autoMocker.CreateInstance<TempDataDictionary>();

        FlashNotificationExtensions.SetFlashSuccess(mockController.Object, "Example flash message.", "Example heading");

        string? capturedFlashNotification = mockController.Object.TempData[FlashNotificationExtensions.TempDataKey] as string;
        Assert.IsNotNull(capturedFlashNotification);

        var viewModel = JsonSerializer.Deserialize<FlashNotificationViewModel>(capturedFlashNotification);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(NotificationBannerType.Success, viewModel.Type);
        Assert.AreEqual("Example heading", viewModel.Heading);
        Assert.AreEqual("Example flash message.", viewModel.Message);
    }

    #endregion
}
