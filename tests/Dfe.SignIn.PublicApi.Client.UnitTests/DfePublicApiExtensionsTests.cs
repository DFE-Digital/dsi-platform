using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class DfePublicApiExtensionsTests
{
    #region SetupDfePublicApiClient(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupDfePublicApiClient_Throws_WhenServicesArgumentIsNull()
    {
        DfePublicApiExtensions.SetupDfePublicApiClient(null!);
    }

    [TestMethod]
    public void SetupDfePublicApiClient_RegistersPublicApiClientServices()
    {
        var services = new ServiceCollection();

        services.SetupDfePublicApiClient();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(PublicApiBearerTokenHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.ServiceType == typeof(IHttpClientFactory)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IPayloadVerifier)
            )
        );
    }

    [TestMethod]
    public void SetupDfePublicApiClient_ConfiguresDefaultCacheOptions()
    {
        var services = new ServiceCollection();

        services.SetupDfePublicApiClient();

        var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
        var options = provider.GetRequiredService<IOptions<PublicKeyCacheOptions>>();

        var expectedTtl = new TimeSpan(hours: 24, minutes: 0, seconds: 0);
        Assert.AreEqual(expectedTtl, options.Value.TTL);

        var expectedMaximumRefreshInterval = new TimeSpan(hours: 0, minutes: 10, seconds: 0);
        Assert.AreEqual(expectedMaximumRefreshInterval, options.Value.MaximumRefreshInterval);
    }

    [TestMethod]
    public void SetupDfePublicApiClient_RegistersSupportingInternalServices()
    {
        var services = new ServiceCollection();

        services.SetupDfePublicApiClient();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IPublicKeyCache)
            )
        );
    }

    #endregion
}
