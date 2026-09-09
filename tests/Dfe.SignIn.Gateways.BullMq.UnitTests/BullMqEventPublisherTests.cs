using BullMQ;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Messaging;
using Dfe.SignIn.Gateways.BullMq.Models;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests;

[TestClass]
public sealed class BullMqEventPublisherTests
{
    private static BullMqEventPublisher CreateSut(
        AutoMocker autoMocker,
        MessagingSettings? messagingSettings = null)
    {
        autoMocker.Use(Options.Create(messagingSettings ?? new MessagingSettings { Enabled = true }));
        return autoMocker.CreateInstance<BullMqEventPublisher>();
    }

    [TestMethod]
    public async Task PublishAsync_WhenEventIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var sut = CreateSut(autoMocker);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => sut.PublishAsync<UserUpdatedEvent>(null!));
    }

    [TestMethod]
    public async Task PublishAsync_WhenMessagingDisabled_DoesNotEnqueue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var sut = CreateSut(autoMocker, new MessagingSettings { Enabled = false });

        var @event = new UserUpdatedEvent {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        };

        // Act
        await sut.PublishAsync(@event);

        // Assert
        autoMocker.GetMock<IBullMqQueueFactory>()
            .Verify(x => x.GetQueue(It.IsAny<string>()), Times.Never);
        autoMocker.GetMock<IBullMqQueueFactory>()
            .Verify(x => x.GetDefaultJobOptions(), Times.Never);
    }

    [TestMethod]
    public async Task PublishAsync_WhenEventTypeUnsupported_ThrowsNotSupportedException()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var sut = CreateSut(autoMocker);

        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(()
            => sut.PublishAsync(new UnsupportedIntegrationEvent()));

        Assert.Contains(nameof(UnsupportedIntegrationEvent), ex.Message);
    }

    [TestMethod]
    public async Task PublishAsync_WhenUserUpdatedEvent_EnqueuesToExpectedQueue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var queueMock = new Mock<IBullMqQueue>();
        var jobOptions = new JobsOptions();
        SafeUserPayload? capturedPayload = null;
        string? capturedJobName = null;
        JobsOptions? capturedOptions = null;

        queueMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<JobsOptions?>()))
            .Callback<string, object, JobsOptions?>((name, data, options) => {
                capturedJobName = name;
                capturedPayload = data as SafeUserPayload;
                capturedOptions = options;
            })
            .ReturnsAsync("job-123");

        autoMocker.GetMock<IBullMqQueueFactory>()
            .Setup(x => x.GetQueue("userupdated_v1"))
            .Returns(queueMock.Object);
        autoMocker.GetMock<IBullMqQueueFactory>()
            .Setup(x => x.GetDefaultJobOptions())
            .Returns(jobOptions);

        var sut = CreateSut(autoMocker);
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var @event = new UserUpdatedEvent {
            UserId = userId,
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 2,
        };

        // Act
        await sut.PublishAsync(@event);

        // Assert
        Assert.AreEqual("userupdated_v1", capturedJobName);
        Assert.IsNotNull(capturedPayload);
        Assert.AreEqual(userId.ToString(), capturedPayload.Sub);
        Assert.AreEqual("user@example.com", capturedPayload.Email);
        Assert.AreEqual("Ada", capturedPayload.GivenName);
        Assert.AreEqual("Lovelace", capturedPayload.FamilyName);
        Assert.AreEqual((short)2, capturedPayload.Status);
        Assert.AreSame(jobOptions, capturedOptions);
        queueMock.Verify(
            x => x.AddAsync("userupdated_v1", It.IsAny<object>(), jobOptions),
            Times.Once);
    }

    [TestMethod]
    public async Task PublishAsync_WhenEnqueueFails_LogsErrorAndRethrows()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var capturedLogs = new List<string>();
        autoMocker.Use(LoggerMocking.GetMockToCaptureLogs<BullMqEventPublisher>(capturedLogs.Add).Object);

        var queueMock = new Mock<IBullMqQueue>();
        queueMock
            .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<JobsOptions?>()))
            .ThrowsAsync(new InvalidOperationException("redis unavailable"));

        autoMocker.GetMock<IBullMqQueueFactory>()
            .Setup(x => x.GetQueue(It.IsAny<string>()))
            .Returns(queueMock.Object);
        autoMocker.GetMock<IBullMqQueueFactory>()
            .Setup(x => x.GetDefaultJobOptions())
            .Returns(new JobsOptions());

        var sut = CreateSut(autoMocker);
        var @event = new UserUpdatedEvent {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        };

        // Act & Assert
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => sut.PublishAsync(@event));

        Assert.AreEqual("redis unavailable", ex.Message);
        Assert.IsTrue(capturedLogs.Exists(log =>
            log.Contains("Error", StringComparison.OrdinalIgnoreCase)
            && log.Contains(@event.EventId.ToString(), StringComparison.Ordinal)));
    }

    private sealed class UnsupportedIntegrationEvent : IIntegrationEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    }
}
