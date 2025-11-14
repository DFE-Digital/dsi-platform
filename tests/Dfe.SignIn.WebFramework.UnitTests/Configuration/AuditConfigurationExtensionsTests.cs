using Dfe.SignIn.Core.Interfaces.Audit;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class AuditConfigurationExtensionsTests
{
    #region SetupAuditContext(IServiceCollection)

    [TestMethod]
    public void SetupAuditContext_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => AuditConfigurationExtensions.SetupAuditContext(null!));
    }

    [TestMethod]
    public void SetupAuditContext_AddsExpectedServices()
    {
        var services = new ServiceCollection();

        AuditConfigurationExtensions.SetupAuditContext(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IHttpContextAccessor)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IAuditContextBuilder)
            )
        );
    }

    #endregion
}
