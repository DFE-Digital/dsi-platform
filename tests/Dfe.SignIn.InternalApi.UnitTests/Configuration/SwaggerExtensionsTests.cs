using Dfe.SignIn.InternalApi.Configuration;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class SwaggerExtensionsTests
{
    [TestMethod]
    public void SetupSwagger_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SwaggerExtensions.SetupSwagger(
                services: null!
            ));
    }
}
