using Microsoft.Extensions.DependencyInjection;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify.UnitTests;

[TestClass]
public sealed class GovNotifyExtensionsTests
{
    #region AddGovNotify(IServiceCollection)

    [TestMethod]
    public void AddGovNotify_Throw_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => GovNotifyExtensions.AddGovNotify(services: null!));
    }

    [TestMethod]
    public void AddGovNotify_RegistersGovNotifyClient()
    {
        var services = new ServiceCollection();

        GovNotifyExtensions.AddGovNotify(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IAsyncNotificationClient)
            )
        );
    }

    #endregion
}
