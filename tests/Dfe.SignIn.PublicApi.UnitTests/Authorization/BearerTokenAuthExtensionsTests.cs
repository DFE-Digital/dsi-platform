using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.UnitTests.Authorization.Fakes;

namespace Dfe.SignIn.PublicApi.UnitTests.Authorization;

[TestClass]
public sealed class BearerTokenAuthExtensionsTests
{
    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Throws_WhenBuilderArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => BearerTokenAuthExtensions.UseBearerTokenAuthMiddleware(
                builder: null!
            ));
    }

    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Should_Be_Registered()
    {
        var fakeApplicationBuilder = new FakeApplicationBuilder();
        fakeApplicationBuilder.UseBearerTokenAuthMiddleware();

        Assert.AreEqual(1, fakeApplicationBuilder.Middleware.Count);
    }
}
