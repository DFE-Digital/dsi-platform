using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    #region SetupInternalApiClient(IServiceCollection, TokenCredential)

    [TestMethod]
    public void SetupInternalApiClient_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceCollectionExtensions.SetupInternalApiClient(
                services: null!,
                credential: new Mock<TokenCredential>().Object
            ));
    }

    [TestMethod]
    public void SetupInternalApiClient_Throws_WhenCredentialArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceCollectionExtensions.SetupInternalApiClient(
                services,
                credential: null!
            ));
    }

    [TestMethod]
    public void SetupInternalApiClient_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        services.AddInteractionFramework();

        var result = ServiceCollectionExtensions.SetupInternalApiClient(
            services,
            credential: new Mock<TokenCredential>().Object
        );

        Assert.AreSame(services, result);
    }

    #endregion
}
