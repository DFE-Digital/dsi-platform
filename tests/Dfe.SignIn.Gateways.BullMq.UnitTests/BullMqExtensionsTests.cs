using Dfe.SignIn.Core.Interfaces.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests;

[TestClass]
public sealed class BullMqExtensionsTests
{
    [TestMethod]
    public void AddBullMqServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBullMqServices();

        // Assert
        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddBullMqServices_RegistersExpectedServiceDescriptors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBullMqServices();

        // Assert
        var factoryDescriptor = services.Single(d => d.ServiceType == typeof(IBullMqQueueFactory));
        Assert.AreEqual(ServiceLifetime.Singleton, factoryDescriptor.Lifetime);
        Assert.AreEqual(typeof(BullMqQueueFactory), factoryDescriptor.ImplementationType);

        var publisherDescriptor = services.Single(d => d.ServiceType == typeof(IEventPublisher));
        Assert.AreEqual(ServiceLifetime.Singleton, publisherDescriptor.Lifetime);
        Assert.AreEqual(typeof(BullMqEventPublisher), publisherDescriptor.ImplementationType);
    }

    [TestMethod]
    public void AddBullMqServices_WhenValidConfig_RewritesConnectionStringWithDatabaseIndex()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["BullMq:ConnectionString"] = "localhost:6379",
                ["BullMq:DatabaseIndex"] = "7",
                ["BullMq:RemoveOnCompleteAgeSeconds"] = "3600",
                ["BullMq:RemoveOnCompleteCount"] = "50",
                ["BullMq:RemoveOnFailAgeSeconds"] = "43200",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddBullMqServices();

        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<BullMqSettings>>().Value;

        // Assert
        var parsed = ConfigurationOptions.Parse(options.ConnectionString);
        Assert.AreEqual(7, parsed.DefaultDatabase);
        Assert.AreEqual(7, options.DatabaseIndex);
        Assert.AreEqual(BullMqSettings.SectionName, "BullMq");
    }

    [TestMethod]
    public void AddBullMqServices_WhenConnectionStringMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["BullMq:DatabaseIndex"] = "4",
                ["BullMq:RemoveOnCompleteAgeSeconds"] = "3600",
                ["BullMq:RemoveOnCompleteCount"] = "50",
                ["BullMq:RemoveOnFailAgeSeconds"] = "43200",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddBullMqServices();

        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(()
            => _ = provider.GetRequiredService<IOptions<BullMqSettings>>().Value);

        Assert.AreEqual("Redis connection string is not configured.", ex.Message);
    }

    [TestMethod]
    public void AddBullMqServices_WhenConnectionStringBlank_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["BullMq:ConnectionString"] = "   ",
                ["BullMq:DatabaseIndex"] = "4",
                ["BullMq:RemoveOnCompleteAgeSeconds"] = "3600",
                ["BullMq:RemoveOnCompleteCount"] = "50",
                ["BullMq:RemoveOnFailAgeSeconds"] = "43200",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddBullMqServices();

        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.ThrowsExactly<InvalidOperationException>(()
            => _ = provider.GetRequiredService<IOptions<BullMqSettings>>().Value);

        Assert.AreEqual("Redis connection string is not configured.", ex.Message);
    }
}
