using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class HomeControllerTests
{
    private static HomeController CreateController(
        AutoMocker autoMocker,
        GetPendingChangeEmailAddressResponse pendingChangeEmailAddressResponse)
    {
        autoMocker.MockResponse<GetPendingChangeEmailAddressRequest>(pendingChangeEmailAddressResponse);

        var controller = autoMocker.CreateInstance<HomeController>();

        var httpContext = new DefaultHttpContext();
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

        return controller;
    }

    #region Index()

    [TestMethod]
    public async Task Index_PresentsDetailsFromUserProfileFeature()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(
            autoMocker,
            pendingChangeEmailAddressResponse: new() { }
        );

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<HomeViewModel>(result);
        Assert.AreEqual("Alex Johnson", viewModel.FullName);
        Assert.AreEqual("Software Developer", viewModel.JobTitle);
        Assert.AreEqual("alex.johnson@example.com", viewModel.EmailAddress);
    }

    [TestMethod]
    public async Task Index_OmitsPendingEmailAddress_WhenNoChangePending()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(
            autoMocker,
            pendingChangeEmailAddressResponse: new() { }
        );

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<HomeViewModel>(result);
        Assert.IsNull(viewModel.PendingEmailAddress);
    }

    [TestMethod]
    public async Task Index_PresentsPendingEmailAddress_WhenChangeIsPending()
    {
        var autoMocker = new AutoMocker();
        var controller = CreateController(
            autoMocker,
            pendingChangeEmailAddressResponse: new() {
                PendingChangeEmailAddress = new() {
                    UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
                    NewEmailAddress = "alex.bradford@example.com",
                    VerificationCode = "12345",
                    ExpiryTimeUtc = new DateTime(2025, 05, 02),
                },
            }
        );

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<HomeViewModel>(result);
        Assert.AreEqual("alex.bradford@example.com", viewModel.PendingEmailAddress);
    }

    #endregion
}
