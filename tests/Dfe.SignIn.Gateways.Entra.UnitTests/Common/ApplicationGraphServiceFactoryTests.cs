using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.Entra.UnitTests.Common;

[TestClass]
public sealed class ApplicationGraphServiceFactoryTests
{
    [TestMethod]
    public void CreateClient_WhenValidOptions_ReturnsGraphServiceClient()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        });

        var factory = new ApplicationGraphServiceFactory(options);

        // Act
        var client = factory.CreateClient();

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void CreateClient_WhenTenantIdMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        });

        var factory = new ApplicationGraphServiceFactory(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(factory.CreateClient);
        Assert.AreEqual("Entra TenantId is not configured.", ex.Message);
    }

    [TestMethod]
    public void CreateClient_WhenClientIdMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "   ",
            ClientSecret = "test-client-secret"
        });

        var factory = new ApplicationGraphServiceFactory(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(factory.CreateClient);
        Assert.AreEqual("Entra ClientId is not configured.", ex.Message);
    }

    [TestMethod]
    public void CreateClient_WhenClientSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = ""
        });

        var factory = new ApplicationGraphServiceFactory(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(factory.CreateClient);
        Assert.AreEqual("Entra ClientSecret is not configured.", ex.Message);
    }
}
