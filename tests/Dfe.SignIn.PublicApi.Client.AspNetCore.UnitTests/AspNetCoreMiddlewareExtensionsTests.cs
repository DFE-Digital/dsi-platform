using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class AspNetCoreMiddlewareExtensionsTests
{
    #region SetupSelectOrganisationMiddleware(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationMiddleware_Throws_WhenServicesArgumentIsNull()
    {
        AspNetCoreMiddlewareExtensions.SetupSelectOrganisationMiddleware(null!);
    }

    [TestMethod]
    public void SetupSelectOrganisationMiddleware_ReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.SetupSelectOrganisationMiddleware();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void SetupSelectOrganisationMiddleware_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationMiddleware();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(AdaptedSelectOrganisationMiddleware)
            )
        );
    }

    #endregion

    #region UseSelectOrganisationMiddleware(IApplicationBuilder)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UseSelectOrganisationMiddleware_Throws_WhenAppArgumentIsNull()
    {
        AspNetCoreMiddlewareExtensions.UseSelectOrganisationMiddleware(null!);
    }

    [TestMethod]
    public void UseSelectOrganisationMiddleware_RegistersMiddleware()
    {
        var autoMocker = new AutoMocker();
        var mockApp = autoMocker.GetMock<IApplicationBuilder>();

        mockApp.Object.UseSelectOrganisationMiddleware();

        mockApp.Verify(
            mock => mock.Use(
                It.IsAny<Func<RequestDelegate, RequestDelegate>>()
            ),
            Times.Once
        );
    }

    #endregion
}
