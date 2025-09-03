using Dfe.SignIn.Base.Framework;
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
            ContractType = typeof(IInteractor<ExampleRequest>),
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
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = requesterType,
        };

        bool result = descriptor.IsFor(NodeApiName.Directories);

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
