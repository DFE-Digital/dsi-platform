using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
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

    [DataTestMethod]
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
            => ServiceCollectionExtensions.SetupNodeApiClient(
                services: null!,
                apiNames: []
            ));
    }

    [TestMethod]
    public void SetupNodeApiClient_Throws_WhenApiNamesArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => services.SetupNodeApiClient(
                apiNames: null!
            ));
    }

    #endregion
}
