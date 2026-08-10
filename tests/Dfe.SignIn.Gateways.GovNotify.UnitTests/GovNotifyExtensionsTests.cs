using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify.UnitTests;

[TestClass]
public sealed class GovNotifyExtensionsTests
{
    #region AddGovNotify(IServiceCollection)

    [TestMethod]
    public void AddGovNotify_Throw_WhenServicesArgumentIsNull()
    {
        var configurationMock = new Mock<IConfiguration>();
        Assert.ThrowsExactly<ArgumentNullException>(()
            => GovNotifyExtensions.AddGovNotify(services: null!, configurationMock.Object));
    }

    [TestMethod]
    public void AddGovNotify_RegistersGovNotifyClient()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();

        GovNotifyExtensions.AddGovNotify(services, configurationMock.Object);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IAsyncNotificationClient)
            )
        );
    }

    #endregion
}
