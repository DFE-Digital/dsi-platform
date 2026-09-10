using BullMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests;

[TestClass]
public sealed class BullMqQueueFactoryTests
{
    private static BullMqSettings DefaultSettings() => new() {
        ConnectionString = "localhost:6379",
        DatabaseIndex = 4,
        RemoveOnCompleteAgeSeconds = 3600,
        RemoveOnCompleteCount = 50,
        RemoveOnFailAgeSeconds = 12 * 3600,
    };

    private static BullMqQueueFactory CreateSut(
        Func<string, IBullMqQueue>? createQueue = null,
        BullMqSettings? settings = null,
        ILogger<BullMqQueueFactory>? logger = null)
    {
        settings ??= DefaultSettings();
        createQueue ??= _ => Mock.Of<IBullMqQueue>();

        return new BullMqQueueFactory(
            Options.Create(settings),
            logger ?? NullLogger<BullMqQueueFactory>.Instance,
            createQueue);
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
    public void GetQueue_CreatesQueueViaFactoryAndReturnsIt()
    {
        // Arrange
        var expected = Mock.Of<IBullMqQueue>();
        string? capturedName = null;
        var sut = CreateSut(name => {
            capturedName = name;
            return expected;
        });

        // Act
        var queue = sut.GetQueue("userupdated_v1");

        // Assert
        Assert.AreSame(expected, queue);
        Assert.AreEqual("userupdated_v1", capturedName);
    }

    [TestMethod]
    public void GetQueue_CachesQueuesUsingOrdinalIgnoreCase()
    {
        // Arrange
        var createCount = 0;
        var sut = CreateSut(_ => {
            createCount++;
            return Mock.Of<IBullMqQueue>();
        });

        // Act
        var first = sut.GetQueue("UserUpdated_V1");
        var second = sut.GetQueue("userupdated_v1");

        // Assert
        Assert.AreSame(first, second);
        Assert.AreEqual(1, createCount);
    }

    [TestMethod]
    public void GetQueue_LogsInitialisation()
    {
        // Arrange
        var capturedLogs = new List<string>();
        var logger = LoggerMocking.GetMockToCaptureLogs<BullMqQueueFactory>(capturedLogs.Add);
        var sut = CreateSut(
            createQueue: _ => Mock.Of<IBullMqQueue>(),
            settings: new BullMqSettings {
                ConnectionString = "localhost:6379",
                DatabaseIndex = 7,
                RemoveOnCompleteAgeSeconds = 3600,
                RemoveOnCompleteCount = 50,
                RemoveOnFailAgeSeconds = 12 * 3600,
            },
            logger: logger.Object);

        // Act
        _ = sut.GetQueue("userupdated_v1");

        // Assert
        Assert.IsTrue(capturedLogs.Exists(log =>
            log.Contains("userupdated_v1", StringComparison.Ordinal)
            && log.Contains('7')));
    }

    [TestMethod]
    public void GetDefaultJobOptions_MapsSettingsToJobsOptions()
    {
        // Arrange
        var sut = CreateSut(settings: new BullMqSettings {
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

    [TestMethod]
    public async Task DisposeAsync_ClosesCreatedQueues()
    {
        // Arrange
        var queueMock = new Mock<IBullMqQueue>();
        queueMock.Setup(x => x.CloseAsync()).Returns(Task.CompletedTask);
        var sut = CreateSut(_ => queueMock.Object);
        _ = sut.GetQueue("userupdated_v1");

        // Act
        await sut.DisposeAsync();

        // Assert
        queueMock.Verify(x => x.CloseAsync(), Times.Once);
    }

    [TestMethod]
    public async Task DisposeAsync_WhenCloseThrows_LogsWarningAndContinues()
    {
        // Arrange
        var capturedLogs = new List<string>();
        var logger = LoggerMocking.GetMockToCaptureLogs<BullMqQueueFactory>(capturedLogs.Add);

        var failingQueue = new Mock<IBullMqQueue>();
        failingQueue.Setup(x => x.CloseAsync()).ThrowsAsync(new InvalidOperationException("close failed"));

        var succeedingQueue = new Mock<IBullMqQueue>();
        succeedingQueue.Setup(x => x.CloseAsync()).Returns(Task.CompletedTask);

        var sut = CreateSut(
            name => name == "queue-a" ? failingQueue.Object : succeedingQueue.Object,
            logger: logger.Object);

        _ = sut.GetQueue("queue-a");
        _ = sut.GetQueue("queue-b");

        // Act
        await sut.DisposeAsync();

        // Assert
        failingQueue.Verify(x => x.CloseAsync(), Times.Once);
        succeedingQueue.Verify(x => x.CloseAsync(), Times.Once);
        Assert.IsTrue(capturedLogs.Exists(log =>
            log.Contains("Warning", StringComparison.OrdinalIgnoreCase)
            && log.Contains("closing BullMQ queue", StringComparison.OrdinalIgnoreCase)),
            $"Expected a close-failure warning. Logs: {string.Join(" | ", capturedLogs)}");
    }
}
