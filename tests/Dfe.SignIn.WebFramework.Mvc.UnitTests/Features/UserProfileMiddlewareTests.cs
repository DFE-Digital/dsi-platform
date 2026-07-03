using System.Security.Claims;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Features;

[TestClass]
public sealed class UserProfileMiddlewareTests
{
    [TestMethod]
    public async Task Ignores_WhenUserProfileFeatureAlreadySet()
    {
        var autoMocker = new AutoMocker();
        var middleware = autoMocker.CreateInstance<UserProfileMiddleware>();

        var context = new DefaultHttpContext();
        var featureBefore = Activator.CreateInstance<UserProfileFeature>();
        context.Features.Set<IUserProfileFeature>(featureBefore);

        await middleware.InvokeAsync(context);

        var featureAfter = context.Features.Get<IUserProfileFeature>();
        Assert.AreSame(featureBefore, featureAfter);
    }

    [TestMethod]
    public async Task DoesNotFetchUserProfile_WhenUserIsNotAuthenticated()
    {
        var autoMocker = new AutoMocker();
        var middleware = autoMocker.CreateInstance<UserProfileMiddleware>();

        await middleware.InvokeAsync(new DefaultHttpContext());

        autoMocker.Verify<IInteractionDispatcher, InteractionTask>(x =>
            x.DispatchAsync(
                It.IsAny<InteractionContext<Core.Contracts.Users.GetUserProfileRequest>>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task FetchesUserProfile_WhenUserIsAuthenticated()
    {
        var autoMocker = new AutoMocker();

        var userId = Guid.Parse("dbb88cbb-c9e6-4d78-843f-d761d14444c8");

        // Ensure the IUsersApiClient returns the expected profile when called by the middleware.
        var response = new GetUserProfileResponseA {
            IsEntra = true,
            IsInternalUser = true,
            FirstName = "Alex",
            LastName = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Engineer",
        };

        autoMocker.GetMock<IUsersApiClient>()
            .Setup(x => x.GetUserProfile(It.Is<GetUserProfileRequestA>(r => r.UserId == userId)))
            .ReturnsAsync(response);

        var middleware = autoMocker.CreateInstance<UserProfileMiddleware>();
        var context = new DefaultHttpContext {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            ], "TestAuthenticationType")),
        };

        await middleware.InvokeAsync(context);

        var userProfileFeature = context.Features.Get<IUserProfileFeature>();
        Assert.IsNotNull(userProfileFeature);
        Assert.AreEqual(userId, userProfileFeature.UserId);
        Assert.IsTrue(userProfileFeature.IsEntra);
        Assert.IsTrue(userProfileFeature.IsInternalUser);
        Assert.AreEqual("Alex", userProfileFeature.FirstName);
        Assert.AreEqual("Johnson", userProfileFeature.LastName);
        Assert.AreEqual("alex.johnson@example.com", userProfileFeature.EmailAddress);
        Assert.AreEqual("Software Engineer", userProfileFeature.JobTitle);
    }

    [TestMethod]
    public async Task InvokesNextMiddleware()
    {
        var autoMocker = new AutoMocker();

        bool wasNextCalled = false;
        autoMocker.Use<RequestDelegate>(_ => {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = autoMocker.CreateInstance<UserProfileMiddleware>();

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.IsTrue(wasNextCalled);
    }
}
