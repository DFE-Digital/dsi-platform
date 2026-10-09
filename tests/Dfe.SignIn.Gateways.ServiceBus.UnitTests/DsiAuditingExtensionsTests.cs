using Azure.Core;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Interfaces.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class DsiAuditingExtensionsTests
{
    [TestMethod]
    public void AddDsiAuditing_Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();
        var tokenCredential = new Mock<TokenCredential>().Object;
        var environment = new Mock<IHostEnvironment>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddDsiAuditing(null!, configuration, tokenCredential, environment));
    }

    [TestMethod]
    public void AddDsiAuditing_Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;
        var environment = new Mock<IHostEnvironment>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddDsiAuditing(services, null!, tokenCredential, environment));
    }

    [TestMethod]
    public void AddDsiAuditing_Throws_WhenTokenCredentialArgumentIsNull()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = new Mock<IHostEnvironment>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddDsiAuditing(services, configuration, null!, environment));
    }

    [TestMethod]
    public void AddDsiAuditing_Throws_WhenEnvironmentArgumentIsNull()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var tokenCredential = new Mock<TokenCredential>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => ServiceBusExtensions.AddDsiAuditing(services, configuration, tokenCredential, null!));
    }

    [TestMethod]
    public void AddDsiAuditing_RegistersAuditContextAndWriter()
    {
        var services = new ServiceCollection();
        var tokenCredential = new Mock<TokenCredential>().Object;
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Local");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("ServiceBus:Namespace", "fake-fully-qualified-namespace"),
                new("ServiceBus:AuditTopic:TopicName", "audit-topic"),
            ])
            .Build();

        ServiceBusExtensions.AddDsiAuditing(services, configuration, tokenCredential, environment.Object);

        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IAuditContextBuilder)));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IAuditWriter)));
    }
}
