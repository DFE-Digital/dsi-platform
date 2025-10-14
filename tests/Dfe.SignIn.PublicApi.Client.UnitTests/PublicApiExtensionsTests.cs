using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class PublicApiExtensionsTests
{
    #region SetupDfePublicApiClient(IServiceCollection)

    [TestMethod]
    public void SetupDfePublicApiClient_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => PublicApiExtensions.SetupDfePublicApiClient(null!));
    }

    [TestMethod]
    public void SetupDfePublicApiClient_ReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.SetupDfePublicApiClient();

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void SetupDfePublicApiClient_RegistersInteractionFramework()
    {
        var services = new ServiceCollection();

        services.SetupDfePublicApiClient();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionDispatcher)
            )
        );
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

        Assert.IsTrue(
            services.HasInteractor<CreateSelectOrganisationSessionApiRequest>()
        );
    }

    #endregion
}
