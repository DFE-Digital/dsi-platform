using System.Net;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Web.Profile.Controllers;
using Dfe.SignIn.Web.Profile.Models;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using Refit;

namespace Dfe.SignIn.Web.Profile.UnitTests.Controllers;

[TestClass]
public sealed class HomeControllerTests
{
    private static readonly Guid TestUserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54");

    private static HomeController CreateController(
        AutoMocker autoMocker,
        GetPendingChangeEmailResponse? pendingResponse = null,
        Exception? getPendingException = null)
    {
        var usersApiClientMock = autoMocker.GetMock<IUsersApiClient>();

        if (getPendingException is not null)
        {
            usersApiClientMock
                .Setup(x => x.GetPendingChangeEmail(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(getPendingException);
        }
        else if (pendingResponse is not null)
        {
            usersApiClientMock
                .Setup(x => x.GetPendingChangeEmail(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pendingResponse);
        }
        else
        {
            // No pending change = 404 from API
            var notFoundException = ApiException.Create(
                new HttpRequestMessage(),
                HttpMethod.Get,
                new HttpResponseMessage(HttpStatusCode.NotFound),
                new RefitSettings()).Result;

            usersApiClientMock
                .Setup(x => x.GetPendingChangeEmail(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(notFoundException);
        }

        var controller = autoMocker.CreateInstance<HomeController>();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = TestUserId,
            IsEntra = false,
            IsInternalUser = false,
            FirstName = "Alex",
            LastName = "Johnson",
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
        var controller = CreateController(autoMocker);

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
        var controller = CreateController(autoMocker);

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
            pendingResponse: new GetPendingChangeEmailResponse {
                NewEmailAddress = "alex.bradford@example.com",
                CreatedAtUtc = DateTime.UtcNow,
                ExpiryTimeUtc = DateTime.UtcNow.AddHours(1),
                HasExpired = false,
            }
        );

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<HomeViewModel>(result);
        Assert.AreEqual("alex.bradford@example.com", viewModel.PendingEmailAddress);
    }

    [TestMethod]
    public async Task Index_OmitsPendingEmailAddress_AndRendersPage_WhenApiReturnsError()
    {
        var autoMocker = new AutoMocker();
        var internalServerErrorException = ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new RefitSettings()).Result;

        var controller = CreateController(autoMocker, getPendingException: internalServerErrorException);

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<HomeViewModel>(result);
        Assert.IsNull(viewModel.PendingEmailAddress);
    }

    #endregion
}
