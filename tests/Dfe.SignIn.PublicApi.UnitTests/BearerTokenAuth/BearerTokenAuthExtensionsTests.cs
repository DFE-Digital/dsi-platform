using Dfe.SignIn.PublicApi.BearerTokenAuth;
using Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth.Fakes;

namespace Dfe.SignIn.PublicApi.UnitTests.BearerTokenAuth;

[TestClass]
public class BearerTokenAuthExtensionsTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UseBearerTokenAuthMiddleware_Throws_WhenBuilderArgumentIsNull()
    {
        BearerTokenAuthExtensions.UseBearerTokenAuthMiddleware(
            builder: null!
        );
    }

    [TestMethod]
    public void UseBearerTokenAuthMiddleware_Should_Be_Registered()
    {
        var fakeApplicationBuilder = new FakeApplicationBuilder();
        fakeApplicationBuilder.UseBearerTokenAuthMiddleware();

        Assert.AreEqual(1, fakeApplicationBuilder.Middleware.Count);
    }
}
