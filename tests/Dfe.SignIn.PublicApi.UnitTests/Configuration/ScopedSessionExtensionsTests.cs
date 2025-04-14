using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.ScopedSession;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class ScopedSessionExtensionsTests
{
    #region SetupScopedSession(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
        ScopedSessionExtensions.SetupScopedSession(
            services: null!
        );
    }

    [TestMethod]
    public void SetupScopedSession_AddsScopedSessionReaderService()
    {
        var services = new ServiceCollection();

        services.SetupScopedSession();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(IScopedSessionReader)
            )
        );
    }

    [TestMethod]
    public void SetupScopedSession_AddsScopedSessionWriterService()
    {
        var services = new ServiceCollection();

        services.SetupScopedSession();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType == typeof(IScopedSessionWriter)
            )
        );
    }

    #endregion
}
