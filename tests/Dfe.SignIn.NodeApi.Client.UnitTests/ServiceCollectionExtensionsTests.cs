using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    #region IsFor(InteractorTypeDescriptor, NodeApiName)

    [TestMethod]
    public void IsFor_ReturnsTrue_WhenAssociatedWithApi()
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = typeof(ExampleApiRequesterForAccessApi)
        };

        bool result = descriptor.IsFor(NodeApiName.Access);

        Assert.IsTrue(result);
    }

    [DataTestMethod]
    [DataRow(typeof(ExampleApiRequesterUnspecifiedApi))]
    [DataRow(typeof(ExampleApiRequesterForAccessApi))]
    public void IsFor_ReturnsFalse_WhenNotAssociatedWithApi(Type requesterType)
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = requesterType,
        };

        bool result = descriptor.IsFor(NodeApiName.Directories);

        Assert.IsFalse(result);
    }

    #endregion

    #region SetupNodeApiClient(IServiceCollection, IEnumerable<NodeApiName>, Action<NodeApiClientOptions>)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupNodeApiClient_Throws_WhenServicesArgumentIsNull()
    {
        ServiceCollectionExtensions.SetupNodeApiClient(
            services: null!,
            apiNames: []
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupNodeApiClient_Throws_WhenApiNamesArgumentIsNull()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient(
            apiNames: null!
        );
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([]);

        // TODO: Test general HttpClient registration.
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersAccessApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([NodeApiName.Access]);

        // TODO: Test for Access api requester registration.
        // Assert.IsTrue(
        //     services.Any(descriptor =>
        //         descriptor.Lifetime == ServiceLifetime.Singleton &&
        //         descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
        //         descriptor.ImplementationType == typeof(ExampleApiRequester)
        //     )
        // );
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersApplicationsApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([NodeApiName.Applications]);

        // TODO: Test for Applications api requester registration.
        // Assert.IsTrue(
        //     services.Any(descriptor =>
        //         descriptor.Lifetime == ServiceLifetime.Singleton &&
        //         descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
        //         descriptor.ImplementationType == typeof(ExampleApiRequester)
        //     )
        // );
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersDirectoriesApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([NodeApiName.Directories]);

        // TODO: Test for Directories api requester registration.
        // Assert.IsTrue(
        //     services.Any(descriptor =>
        //         descriptor.Lifetime == ServiceLifetime.Singleton &&
        //         descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
        //         descriptor.ImplementationType == typeof(ExampleApiRequester)
        //     )
        // );
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersOrganisationsApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([NodeApiName.Organisations]);

        // TODO: Test for Organisations api requester registration.
        // Assert.IsTrue(
        //     services.Any(descriptor =>
        //         descriptor.Lifetime == ServiceLifetime.Singleton &&
        //         descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
        //         descriptor.ImplementationType == typeof(ExampleApiRequester)
        //     )
        // );
    }

    [TestMethod]
    public void SetupNodeApiClient_RegistersSearchApiRequesters()
    {
        var services = new ServiceCollection();

        services.SetupNodeApiClient([NodeApiName.Search]);

        // TODO: Test for Search api requester registration.
        // Assert.IsTrue(
        //     services.Any(descriptor =>
        //         descriptor.Lifetime == ServiceLifetime.Singleton &&
        //         descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
        //         descriptor.ImplementationType == typeof(ExampleApiRequester)
        //     )
        // );
    }

    #endregion
}
