using Azure.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class DataProtectionExtensionsTests
{
    private static TokenCredential CreateTokenCredential()
        => new Mock<TokenCredential>().Object;

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    [TestMethod]
    public void AddDsiDataProtection_NoDataProtectionSection_DoesNothing()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration([]);
        var credential = CreateTokenCredential();

        services.AddDsiDataProtection(configuration, credential, "TestApp");

        Assert.AreEqual(0, services.Count);
    }

    [TestMethod]
    public void AddDsiDataProtection_NullServices_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            DataProtectionExtensions.AddDsiDataProtection(
                null!,
                new ConfigurationBuilder().Build(),
                CreateTokenCredential(),
                "TestApp"
            );
        });
    }

    [TestMethod]
    public void AddDsiDataProtection_NullConfiguration_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            DataProtectionExtensions.AddDsiDataProtection(
                new ServiceCollection(),
                null!,
                CreateTokenCredential(),
                "TestApp"
            );
        });

    }

    [TestMethod]
    public void AddDsiDataProtection_NullCredential_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            DataProtectionExtensions.AddDsiDataProtection(
                new ServiceCollection(),
                new ConfigurationBuilder().Build(),
                null!,
                "TestApp"
            );
        });
    }

    [TestMethod]
    public void AddDsiDataProtection_EmptyApplicationName_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => {
            DataProtectionExtensions.AddDsiDataProtection(
                new ServiceCollection(),
                new ConfigurationBuilder().Build(),
                CreateTokenCredential(),
                ""
            );
        });
    }

    [TestMethod]
    public void AddDsiDataProtection_NullApplicationName_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            DataProtectionExtensions.AddDsiDataProtection(
                new ServiceCollection(),
                new ConfigurationBuilder().Build(),
                CreateTokenCredential(),
                null!
            );
        });
    }

    [TestMethod]
    public void AddDsiDataProtection_UnsupportedStrategy_Throws()
    {
        var services = new ServiceCollection();
        var config = CreateConfiguration(new Dictionary<string, string?> {
            ["DataProtection:Strategy"] = "Redis",
            ["DataProtection:KeyLifetimeInDays"] = "90"
        });

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.AddDsiDataProtection(config, CreateTokenCredential(), "TestApp"));

        Assert.Contains("Unsupported data protection strategy", ex.Message);
    }

    [TestMethod]
    public void AddDsiDataProtection_BlobStorageWithoutOptions_Throws()
    {
        var services = new ServiceCollection();
        var config = CreateConfiguration(new Dictionary<string, string?> {
            ["DataProtection:Strategy"] = "BlobStorage",
            ["DataProtection:KeyLifetimeInDays"] = "90"
        });

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.AddDsiDataProtection(config, CreateTokenCredential(), "TestApp"));

        Assert.Contains("BlobStorage options must be provided", ex.Message);
    }

    [TestMethod]
    public void AddDsiDataProtection_FileSystemWithoutOptions_Throws()
    {
        var services = new ServiceCollection();
        var config = CreateConfiguration(new Dictionary<string, string?> {
            ["DataProtection:Strategy"] = "FileSystem",
            ["DataProtection:KeyLifetimeInDays"] = "90"
        });

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.AddDsiDataProtection(config, CreateTokenCredential(), "TestApp"));

        Assert.Contains("FileSystem options must be provided", ex.Message);
    }

    [TestMethod]
    public void AddDsiDataProtection_WithBlobStorageConfiguration_DoesNotThrow()
    {
        var services = new ServiceCollection();

        var configuration = CreateConfiguration(new Dictionary<string, string?> {
            ["DataProtection:Strategy"] = "BlobStorage",
            ["DataProtection:KeyLifetimeInDays"] = "90",
            ["DataProtection:BlobStorage:StorageAccountUri"] =
                "https://testaccount.blob.core.windows.net/dataprotection/keys.xml"
        });

        services.AddDsiDataProtection(configuration, CreateTokenCredential(), "TestApp");
        var provider = services.BuildServiceProvider();
        var dataProtectionProvider = provider.GetService<IDataProtectionProvider>();

        Assert.IsNotNull(dataProtectionProvider);
    }

    [TestMethod]
    public void AddDsiDataProtection_FileSystemStrategy_RegistersDataProtection()
    {
        var services = new ServiceCollection();
        var config = CreateConfiguration(new Dictionary<string, string?> {
            ["DataProtection:Strategy"] = "FileSystem",
            ["DataProtection:KeyLifetimeInDays"] = "90",
            ["DataProtection:FileSystem:Path"] = "C:\\keys"
        });

        services.AddDsiDataProtection(config, CreateTokenCredential(), "TestApp");

        var provider = services.BuildServiceProvider();
        var protector = provider.GetService<IDataProtectionProvider>();

        Assert.IsNotNull(protector);
    }
}
