using System.Security.Claims;
using Dfe.SignIn.WebFramework.Mvc.Features;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Policies;

[TestClass]
public sealed class InternalUserRequirementHandlerTests
{
    private static IHttpContextAccessor CreateHttpContextWithUserProfileFeature(bool isInternalUser)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IUserProfileFeature>(new UserProfileFeature {
            UserId = Guid.Parse("15eb0a65-2d08-4f96-8dc9-9d77798e6c54"),
            IsEntra = false,
            IsInternalUser = isInternalUser,
            FirstName = "Alex",
            LastName = "Johnson",
            EmailAddress = "alex.johnson@example.com",
            JobTitle = "Software Developer",
        });

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        return accessor.Object;
    }

    [TestMethod]
    public async Task FailsRequirement_WhenUserProfileIsMissing()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var handler = new InternalUserRequirementHandler(accessor.Object);

        var context = new AuthorizationHandlerContext(
            requirements: [
                new InternalUserRequirement { IsInternalUser = false },
            ],
            user: new ClaimsPrincipal(),
            resource: null
        );

        await handler.HandleAsync(context);

        Assert.IsTrue(context.HasFailed);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SucceedsRequirement_WhenRequirementIsMet(bool isInternalUser)
    {
        var handler = new InternalUserRequirementHandler(
            CreateHttpContextWithUserProfileFeature(isInternalUser)
        );

        var context = new AuthorizationHandlerContext(
            requirements: [
                new InternalUserRequirement { IsInternalUser = isInternalUser },
            ],
            user: new ClaimsPrincipal(),
            resource: null
        );

        await handler.HandleAsync(context);

        Assert.IsTrue(context.HasSucceeded);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task DoesNotSucceedsRequirement_WhenRequirementIsNotMet(bool isInternalUser)
    {
        var handler = new InternalUserRequirementHandler(
            CreateHttpContextWithUserProfileFeature(!isInternalUser)
        );

        var context = new AuthorizationHandlerContext(
            requirements: [
                new InternalUserRequirement { IsInternalUser = isInternalUser },
            ],
            user: new ClaimsPrincipal(),
            resource: null
        );

        await handler.HandleAsync(context);

        Assert.IsFalse(context.HasSucceeded);
    }
}
