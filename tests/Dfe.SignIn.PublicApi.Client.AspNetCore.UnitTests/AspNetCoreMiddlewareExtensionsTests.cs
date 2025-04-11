using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class AspNetCoreMiddlewareExtensionsTests
{
    #region UseAuthenticationOrganisationSelectorMiddleware(IApplicationBuilder)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UseAuthenticationOrganisationSelectorMiddleware_Throws_WhenAppArgumentIsNull()
    {
        AspNetCoreMiddlewareExtensions.UseAuthenticationOrganisationSelectorMiddleware(null!);
    }

    [TestMethod]
    public void UseAuthenticationOrganisationSelectorMiddleware_RegistersMiddleware()
    {
        var autoMocker = new AutoMocker();
        var mockApp = autoMocker.GetMock<IApplicationBuilder>();

        mockApp.Object.UseAuthenticationOrganisationSelectorMiddleware();

        mockApp.Verify(
            mock => mock.Use(
                It.IsAny<Func<RequestDelegate, RequestDelegate>>()
            ),
            Times.Once
        );
    }

    #endregion
}
