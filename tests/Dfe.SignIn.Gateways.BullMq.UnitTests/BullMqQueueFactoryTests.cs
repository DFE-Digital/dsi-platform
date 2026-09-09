using BullMQ;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests;

[TestClass]
public sealed class BullMqQueueFactoryTests
{
    private static BullMqQueueFactory CreateSut(BullMqSettings? settings = null)
    {
        settings ??= new BullMqSettings {
            ConnectionString = "localhost:6379",
            DatabaseIndex = 4,
            RemoveOnCompleteAgeSeconds = 3600,
            RemoveOnCompleteCount = 50,
            RemoveOnFailAgeSeconds = 12 * 3600,
        };

        return new BullMqQueueFactory(
            Options.Create(settings),
            NullLogger<BullMqQueueFactory>.Instance);
    }

    [TestMethod]
    public void GetQueue_WhenQueueNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => sut.GetQueue(null!));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void GetQueue_WhenQueueNameIsWhiteSpace_ThrowsArgumentException(string queueName)
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => sut.GetQueue(queueName));
    }

    [TestMethod]
    public void GetDefaultJobOptions_MapsSettingsToJobsOptions()
    {
        // Arrange
        var sut = CreateSut(new BullMqSettings {
            ConnectionString = "localhost:6379",
            DatabaseIndex = 4,
            RemoveOnCompleteAgeSeconds = 100,
            RemoveOnCompleteCount = 10,
            RemoveOnFailAgeSeconds = 200,
        });

        // Act
        var options = sut.GetDefaultJobOptions();

        // Assert
        var removeOnComplete = options.RemoveOnComplete as KeepJobs;
        Assert.IsNotNull(removeOnComplete);
        Assert.AreEqual(100, removeOnComplete.Age);
        Assert.AreEqual(10, removeOnComplete.Count);

        var removeOnFail = options.RemoveOnFail as KeepJobs;
        Assert.IsNotNull(removeOnFail);
        Assert.AreEqual(200, removeOnFail.Age);
    }

    [TestMethod]
    public async Task DisposeAsync_WhenNoQueuesCreated_CompletesSuccessfully()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await sut.DisposeAsync();
    }
}
