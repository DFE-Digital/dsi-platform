using Dfe.SignIn.PublicApi.Configuration;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class SwaggerExtensionsTests
{
    #region SetupSwagger(IServiceCollection)

    [TestMethod]
    public void SetupSwagger_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SwaggerExtensions.SetupSwagger(
                services: null!
            ));
    }

    #endregion
}
