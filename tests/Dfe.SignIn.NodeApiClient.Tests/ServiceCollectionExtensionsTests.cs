using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.NodeApiClient.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Tests;

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

    #region AddNodeApiClient(IServiceCollection, IEnumerable<NodeApiName>, Action<NodeApiClientOptions>)

    [TestMethod]
    public void AddNodeApiClient_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ServiceCollectionExtensions.AddNodeApiClient(null, [], options => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddNodeApiClient_Throws_WhenApiNamesArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddNodeApiClient(null, options => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddNodeApiClient_Throws_WhenSetupActionArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddNodeApiClient([], null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddNodeApiClient_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([], options => { });

        // TODO: Test general HttpClient registration.
    }

    [TestMethod]
    public void AddNodeApiClient_RegistersAccessApiRequesters()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([NodeApiName.Access], options => { });

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
    public void AddNodeApiClient_RegistersApplicationsApiRequesters()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([NodeApiName.Applications], options => { });

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
    public void AddNodeApiClient_RegistersDirectoriesApiRequesters()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([NodeApiName.Directories], options => { });

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
    public void AddNodeApiClient_RegistersOrganisationsApiRequesters()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([NodeApiName.Organisations], options => { });

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
    public void AddNodeApiClient_RegistersSearchApiRequesters()
    {
        var services = new ServiceCollection();

        services.AddNodeApiClient([NodeApiName.Search], options => { });

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
