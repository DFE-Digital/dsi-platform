using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class ClientSessionExtensionsTests
{
    #region SetupScopedSession(IServiceCollection)

    [TestMethod]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ClientSessionExtensions.SetupScopedSession(
                services: null!
            ));
    }

    [TestMethod]
    public void SetupScopedSession_AddsScopedSessionServices()
    {
        var services = new ServiceCollection();

        ClientSessionExtensions.SetupScopedSession(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(IClientSessionWriter)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(IClientSession)
            )
        );
    }

    [TestMethod]
    public void SetupScopedSession_ResolvesConsistentServices()
    {
        var services = new ServiceCollection();

        ClientSessionExtensions.SetupScopedSession(services);

        var provider = services.BuildServiceProvider();

        var clientSessionProvider = provider.GetService<ClientSessionProvider>();
        Assert.IsNotNull(clientSessionProvider);

        var clientSessionWriter = provider.GetService<IClientSessionWriter>();
        Assert.AreSame(clientSessionProvider, clientSessionWriter);

        var clientSession = provider.GetService<IClientSession>();
        Assert.AreSame(clientSessionProvider, clientSession);
    }

    #endregion
}
