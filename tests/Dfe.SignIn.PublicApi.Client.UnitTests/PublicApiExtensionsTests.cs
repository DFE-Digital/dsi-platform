using System.Diagnostics;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiExtensionsTests
{
    [StackTraceHidden]
    private static void AssertHasApiRequester<TRequest, TResponse>(IServiceCollection services)
        where TRequest : class
        where TResponse : class
    {
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<TRequest, TResponse>)
            )
        );
    }

    #region SetupDfePublicApiClient(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupDfePublicApiClient_Throws_WhenServicesArgumentIsNull()
    {
        PublicApiExtensions.SetupDfePublicApiClient(null!);
    }

    [TestMethod]
    public void SetupDfePublicApiClient_ReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.SetupDfePublicApiClient();

        Assert.AreSame(services, result);
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
                (string?)descriptor.ServiceKey == PublicApiConstants.HttpClientKey &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(HttpClient)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IPublicApiClient)
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
    public void SetupDfePublicApiClient_AddsJsonConverters()
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
                descriptor.ServiceType == typeof(TimeProvider)
            )
        );

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IPublicKeyCache)
            )
        );
    }

    [TestMethod]
    public void SetupDfePublicApiClient_RegistersSupportingInternalServicesConditionallyAddsTimeProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new MockTimeProvider(DateTime.UtcNow));

        services.SetupDfePublicApiClient();

        var timeProviderDescriptors = services.Where(descriptor =>
            descriptor.Lifetime == ServiceLifetime.Singleton &&
            (descriptor.ServiceType == typeof(TimeProvider) ||
            descriptor.ServiceType.IsAssignableTo(typeof(TimeProvider)))
        ).ToArray();

        Assert.AreEqual(1, timeProviderDescriptors.Length);
        Assert.AreEqual(typeof(MockTimeProvider), timeProviderDescriptors[0].ServiceType);
    }

    [TestMethod]
    public void SetupDfePublicApiClient_RegistersSelectOrganisationApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupDfePublicApiClient();

        AssertHasApiRequester<
            CreateSelectOrganisationSession_PublicApiRequest,
            CreateSelectOrganisationSession_PublicApiResponse
        >(services);
    }

    #endregion
}
