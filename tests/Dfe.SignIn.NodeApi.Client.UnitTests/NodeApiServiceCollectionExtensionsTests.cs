using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.NodeApi.Client.UnitTests;

[TestClass]
public sealed class NodeApiServiceCollectionExtensionsTests
{
    private static IConfiguration GetMockApiConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("InternalApi:BaseAddress", "http://internal-api.localhost"),
                new("InternalApi:ClientId", "fake-client-id"),
                new("InternalApi:Tenant", "fake-tenant"),
                new("InternalApi:ClientSecret", "fake-client-secret"),
                new("InternalApi:HostUrl", "http://host.localhost"),
                new("InternalApi:Resource", "00000000-0000-0000-0000-000000000000"),
            ])
            .Build();
    }

    #region AreAllRequiredApisAvailable(InteractorTypeDescriptor, IEnumerable<NodeApiName>)

    [TestMethod]
    public void AreAllRequiredApisAvailable_ReturnsTrue_WhenRequiredApiIsAvailable()
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(ExampleApiRequesterForAccessApi)
        };

        bool result = descriptor.AreAllRequiredApisAvailable([NodeApiName.Access]);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void AreAllRequiredApisAvailable_ReturnsTrue_WhenAllRequiredApisAreAvailable()
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(ExampleApiRequesterForAccessAndDirectoriesApi)
        };

        bool result = descriptor.AreAllRequiredApisAvailable([NodeApiName.Access, NodeApiName.Directories]);

        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow(typeof(ExampleApiRequesterUnspecifiedApi))]
    [DataRow(typeof(ExampleApiRequesterForAccessApi))]
    public void AreAllRequiredApisAvailable_ReturnsFalse_WhenNoRequiredApisAreUnavailable(Type requesterType)
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = requesterType,
        };

        bool result = descriptor.AreAllRequiredApisAvailable([NodeApiName.Directories]);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreAllRequiredApisAvailable_ReturnsFalse_WhenSomeRequiredApisAreUnavailable()
    {
        var descriptor = new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(ExampleApiRequesterForAccessAndDirectoriesApi)
        };

        bool result = descriptor.AreAllRequiredApisAvailable([NodeApiName.Access]);

        Assert.IsFalse(result);
    }

    #endregion

    #region SetupNodeApiClient(IServiceCollection, IEnumerable<NodeApiName>, Action<NodeApiClientOptions>)

    [TestMethod]
    public void SetupNodeApiClient_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => NodeApiServiceCollectionExtensions.SetupNodeApiClient(
                services: null!,
                apiNames: [],
                apiConfiguration: GetMockApiConfiguration(),
                credential: new Mock<TokenCredential>().Object
            ));
    }

    [TestMethod]
    public void SetupNodeApiClient_Throws_WhenApiNamesArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => NodeApiServiceCollectionExtensions.SetupNodeApiClient(
                services,
                apiNames: null!,
                apiConfiguration: GetMockApiConfiguration(),
                credential: new Mock<TokenCredential>().Object
            ));
    }

    [TestMethod]
    public void SetupNodeApiClient_Throws_WhenCredentialArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => NodeApiServiceCollectionExtensions.SetupNodeApiClient(
                services,
                apiNames: [],
                apiConfiguration: GetMockApiConfiguration(),
                credential: null!
            ));
    }

    [TestMethod]
    public void SetupNodeApiClient_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = NodeApiServiceCollectionExtensions.SetupNodeApiClient(
            services,
            [NodeApiName.Directories],
            apiConfiguration: GetMockApiConfiguration(),
            credential: new Mock<TokenCredential>().Object
        );

        Assert.AreSame(services, result);
    }

    #endregion
}
