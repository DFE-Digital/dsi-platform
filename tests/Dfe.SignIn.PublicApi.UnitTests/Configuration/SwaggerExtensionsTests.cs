using Dfe.SignIn.PublicApi.Configuration;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class SwaggerExtensionsTests
{
    #region SetupSwagger(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSwagger_Throws_WhenServicesArgumentIsNull()
    {
        SwaggerExtensions.SetupSwagger(
            services: null!
        );
    }

    #endregion
}
