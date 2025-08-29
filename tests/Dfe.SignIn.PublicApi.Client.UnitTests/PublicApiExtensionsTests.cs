using System.Diagnostics;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiExtensionsTests
{
    [StackTraceHidden]
    private static void AssertHasApiRequester<TRequest>(IServiceCollection services)
        where TRequest : class
    {
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<TRequest>)
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

        AssertHasApiRequester<CreateSelectOrganisationSessionApiRequest>(services);
    }

    #endregion
}
