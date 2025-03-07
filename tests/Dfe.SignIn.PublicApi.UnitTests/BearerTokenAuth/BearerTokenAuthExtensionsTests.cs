using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth.Fakes;

namespace Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth;

[TestClass]
public class BearerTokenAuthExtensionsTests
{
    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Throws_WhenBuilderArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => BearerTokenAuthExtensions.UseBearerTokenAuthMiddleware(null, (options) => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Throws_WhenOptionsArgumentIsNull()
    {
        var fakeApplicationBuilder = new FakeApplicationBuilder();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => BearerTokenAuthExtensions.UseBearerTokenAuthMiddleware(fakeApplicationBuilder, null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Should_Be_Registered()
    {
        var fakeApplicationBuilder = new FakeApplicationBuilder();
        fakeApplicationBuilder.UseBearerTokenAuthMiddleware((options) => { });

        Assert.AreEqual(1, fakeApplicationBuilder.Middleware.Count);
    }
}