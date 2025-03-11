using Dfe.SignIn.PublicApi.Configuration;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class SwaggerExtensionsTests
{
    #region SetupSwagger(IServiceCollection)

    [TestMethod]
    public void SetupSwagger_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            SwaggerExtensions.SetupSwagger(
                services: null!
            )
        );
    }

    #endregion
}
