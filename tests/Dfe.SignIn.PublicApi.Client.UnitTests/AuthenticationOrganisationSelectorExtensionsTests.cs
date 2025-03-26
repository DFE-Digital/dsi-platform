using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class AuthenticationOrganisationSelectorExtensionsTests
{
    #region SetupSelectOrganisationFeatures(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupSelectOrganisationFeatures_Throws_WhenServicesArgumentIsNull()
    {
        AuthenticationOrganisationSelectorExtensions.SetupSelectOrganisationFeatures(null!);
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_RegistersGeneralFeatures()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationFeatures();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ISelectOrganisationCallbackProcessor)
            )
        );
    }

    [TestMethod]
    public void SetupSelectOrganisationFeatures_RegistersAuthenticationOrganisationSelector()
    {
        var services = new ServiceCollection();

        services.SetupSelectOrganisationFeatures();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IAuthenticationOrganisationSelector)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IOrganisationClaimManager)
            )
        );
    }

    #endregion

    #region UseAuthenticationOrganisationSelectorMiddleware(IApplicationBuilder)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UseAuthenticationOrganisationSelectorMiddleware_Throws_WhenAppArgumentIsNull()
    {
        AuthenticationOrganisationSelectorExtensions.UseAuthenticationOrganisationSelectorMiddleware(null!);
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
