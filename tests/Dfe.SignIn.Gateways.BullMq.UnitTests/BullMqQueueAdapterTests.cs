using BullMQ;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests;

[TestClass]
public sealed class BullMqQueueAdapterTests
{
    [TestMethod]
    public async Task AddAsync_WhenJobIdPresent_ReturnsJobId()
    {
        // Arrange
        var options = new JobsOptions();
        object? capturedPayload = null;
        var sut = new BullMqQueueAdapter(
            (name, data, jobOptions) => {
                Assert.AreEqual("userupdated_v1", name);
                Assert.AreSame(options, jobOptions);
                capturedPayload = data;
                return Task.FromResult<string?>("job-123");
            },
            () => Task.CompletedTask,
            Mock.Of<ILogger<BullMqQueueAdapter>>());

        // Act
        var jobId = await sut.AddAsync("userupdated_v1", new { Value = 1 }, options);

        // Assert
        Assert.AreEqual("job-123", jobId);
        Assert.IsNotNull(capturedPayload);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task AddAsync_WhenJobIdMissing_LogsWarningAndReturnsEmptyString(string? returnedJobId)
    {
        // Arrange
        var capturedLogs = new List<string>();
        var logger = LoggerMocking.GetMockToCaptureLogs<BullMqQueueAdapter>(capturedLogs.Add);
        var sut = new BullMqQueueAdapter(
            (_, _, _) => Task.FromResult(returnedJobId),
            () => Task.CompletedTask,
            logger.Object);

        // Act
        var jobId = await sut.AddAsync("userupdated_v1", new { }, new JobsOptions());

        // Assert
        Assert.AreEqual(string.Empty, jobId);
        Assert.IsTrue(capturedLogs.Exists(log =>
            log.Contains("Warning", StringComparison.OrdinalIgnoreCase)
            && log.Contains("userupdated_v1", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task CloseAsync_DelegatesToUnderlyingClose()
    {
        // Arrange
        var closed = false;
        var sut = new BullMqQueueAdapter(
            (_, _, _) => Task.FromResult<string?>("job-1"),
            () => {
                closed = true;
                return Task.CompletedTask;
            },
            Mock.Of<ILogger<BullMqQueueAdapter>>());

        // Act
        await sut.CloseAsync();

        // Assert
        Assert.IsTrue(closed);
    }
}
