using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.Entra.UnitTests;

[TestClass]
public sealed class EntraGatewayExtensionsTests
{
    [TestMethod]
    public void AddEntraApplicationServices_RegistersExpectedServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["TestExternalId:TenantId"] = "tenant-123",
                ["TestExternalId:ClientId"] = "client-456",
                ["TestExternalId:ClientSecret"] = "secret-789"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddEntraApplicationServices(settings: settings => {
            var externalSection = configuration.GetSection("TestExternalId");
            settings.TenantId = externalSection.GetValue<string>("TenantId")
                ?? throw new InvalidOperationException("TenantId is not configured");
            settings.ClientId = externalSection.GetValue<string>("ClientId")
                ?? throw new InvalidOperationException("ClientId is not configured");
            settings.ClientSecret = externalSection.GetValue<string>("ClientSecret")
                ?? throw new InvalidOperationException("ClientSecret is not configured");
        });
        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<EntraApplicationSettings>>().Value;
        Assert.AreEqual("tenant-123", options.TenantId);
        Assert.AreEqual("client-456", options.ClientId);
        Assert.AreEqual("secret-789", options.ClientSecret);

        var graphClientProvider = serviceProvider.GetService<IApplicationGraphClientProvider>();
        Assert.IsNotNull(graphClientProvider);
        Assert.IsInstanceOfType<ApplicationGraphClientProvider>(graphClientProvider);

        var emailService = serviceProvider.GetService<IEntraChangeEmailService>();
        Assert.IsNotNull(emailService);
        Assert.IsInstanceOfType<EntraChangeEmailService>(emailService);
    }
}
