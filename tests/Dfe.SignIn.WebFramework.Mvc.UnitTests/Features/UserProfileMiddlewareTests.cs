using System.Security.Claims;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Features;

//TODO [GetUserProfile]: This needs replacing with a call to the user profile service once it is available.For now, we will just log the user id in the audit message.
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

        autoMocker.GetMock<IUsersApiClient>()
            .Verify(x => x.GetUserProfile(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task FetchesUserProfile_WhenUserIsAuthenticated()
    {
        var autoMocker = new AutoMocker();
        var userApiClientMock = autoMocker.GetMock<IUsersApiClient>();
        userApiClientMock
            .Setup(x => x.GetUserProfile(new Guid("dbb88cbb-c9e6-4d78-843f-d761d14444c8")))
            .ReturnsAsync(new GetUserProfileResponse(
                new Guid("dbb88cbb-c9e6-4d78-843f-d761d14444c8"),
                 "Alex",
                 "Johnson",
                 "alex.johnson@example.com",
                 true,
                    true,
                 "Software Engineer",
                 1
                ));

        var middleware = autoMocker.CreateInstance<UserProfileMiddleware>();
        var context = new DefaultHttpContext {
            User = new ClaimsPrincipal([
                new([
                    new(ClaimTypes.NameIdentifier, "dbb88cbb-c9e6-4d78-843f-d761d14444c8"),
                ], "TestAuthenticationType"),
            ]),
        };

        await middleware.InvokeAsync(context);

        var userProfileFeature = context.Features.Get<IUserProfileFeature>();
        Assert.IsNotNull(userProfileFeature);
        Assert.AreEqual(Guid.Parse("dbb88cbb-c9e6-4d78-843f-d761d14444c8"), userProfileFeature.UserId);
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
