using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.Entra.UnitTests.Common;

[TestClass]
public sealed class ApplicationGraphClientProviderTests
{
    [TestMethod]
    public void GetClient_WhenValidOptions_ReturnsGraphServiceClient()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        });

        var provider = new ApplicationGraphClientProvider(options);

        // Act
        var client = provider.GetClient();

        // Assert
        Assert.IsNotNull(client);
    }

    [TestMethod]
    public void GetClient_ReturnsSameInstanceOnSubsequentCalls()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        });

        var provider = new ApplicationGraphClientProvider(options);

        // Act
        var firstClient = provider.GetClient();
        var secondClient = provider.GetClient();

        // Assert
        Assert.AreSame(firstClient, secondClient);
    }

    [TestMethod]
    public void GetClient_WhenTenantIdMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        });

        var provider = new ApplicationGraphClientProvider(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(provider.GetClient);
        Assert.AreEqual("Entra TenantId is not configured.", ex.Message);
    }

    [TestMethod]
    public void GetClient_WhenClientIdMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "   ",
            ClientSecret = "test-client-secret"
        });

        var provider = new ApplicationGraphClientProvider(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(provider.GetClient);
        Assert.AreEqual("Entra ClientId is not configured.", ex.Message);
    }

    [TestMethod]
    public void GetClient_WhenClientSecretMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new EntraApplicationSettings {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = ""
        });

        var provider = new ApplicationGraphClientProvider(options);

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(provider.GetClient);
        Assert.AreEqual("Entra ClientSecret is not configured.", ex.Message);
    }
}
