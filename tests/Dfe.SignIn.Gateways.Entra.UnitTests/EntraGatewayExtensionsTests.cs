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
                ["Entra:TenantId"] = "tenant-123",
                ["Entra:ClientId"] = "client-456",
                ["Entra:ClientSecret"] = "secret-789",
                ["Entra:GraphEndpoint"] = "https://graph.microsoft.com/v1.0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEntraApplicationServices(configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<EntraApplicationOptions>>().Value;
        Assert.AreEqual("tenant-123", options.TenantId);
        Assert.AreEqual("client-456", options.ClientId);
        Assert.AreEqual("secret-789", options.ClientSecret);
        Assert.AreEqual("https://graph.microsoft.com/v1.0", options.GraphEndpoint);

        var factory = provider.GetService<IApplicationGraphServiceFactory>();
        Assert.IsNotNull(factory);
        Assert.IsInstanceOfType<ApplicationGraphServiceFactory>(factory);

        var emailService = provider.GetService<IEntraChangeEmailService>();
        Assert.IsNotNull(emailService);
        Assert.IsInstanceOfType<EntraChangeEmailService>(emailService);
    }
}
