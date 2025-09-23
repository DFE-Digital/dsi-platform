using Azure.Messaging.ServiceBus;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Gateways.ServiceBus.UnitTests;

[TestClass]
public sealed class ServiceBusProcessingServiceTests
{
    #region OnProcessMessageAsync(ProcessMessageEventArgs)

    [TestMethod]
    public async Task OnProcessMessageAsync_ResolvesHandlerWithSubject()
    {
        var autoMocker = new AutoMocker();
        var processingService = autoMocker.CreateInstance<ServiceBusProcessingService>();

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(subject: "subject1");
        var fakeArgs = new ProcessMessageEventArgs(fakeMessage, null!, null!, CancellationToken.None);

        await processingService.OnProcessMessageAsync(fakeArgs);

        autoMocker.Verify<IServiceBusHandlerResolver>(x =>
            x.ResolveHandlers(
                It.Is<string>(subject => subject == "subject1")
            )!,
            Times.Once
        );
    }

    [TestMethod]
    public async Task OnProcessMessageAsync_Completes_WhenNoHandlersAreResolved()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IServiceBusHandlerResolver>()
            .Setup(x => x.ResolveHandlers(
                It.Is<string>(subject => subject == "subject1")
            ))
            .Returns([]);

        var processingService = autoMocker.CreateInstance<ServiceBusProcessingService>();

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(subject: "subject1");
        var fakeArgs = new ProcessMessageEventArgs(fakeMessage, null!, null!, CancellationToken.None);

        try {
            await processingService.OnProcessMessageAsync(fakeArgs);
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    [TestMethod]
    public async Task OnProcessMessageAsync_InvokesResolvedHandler()
    {
        var autoMocker = new AutoMocker();

        var mockMessageHandler = new Mock<IServiceBusMessageHandler>();

        autoMocker.GetMock<IServiceBusHandlerResolver>()
            .Setup(x => x.ResolveHandlers(
                It.Is<string>(subject => subject == "subject1")
            ))
            .Returns([mockMessageHandler.Object]);

        var processingService = autoMocker.CreateInstance<ServiceBusProcessingService>();

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(subject: "subject1");
        var fakeArgs = new ProcessMessageEventArgs(fakeMessage, null!, null!, new CancellationToken());

        await processingService.OnProcessMessageAsync(fakeArgs);

        mockMessageHandler.Verify(x =>
            x.HandleAsync(
                It.Is<ServiceBusReceivedMessage>(p => ReferenceEquals(fakeMessage, p)),
                It.Is<CancellationToken>(p => Equals(fakeArgs.CancellationToken, p))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task OnProcessMessageAsync_DoesNotInvokeResolvedHandler_WhenCancelled()
    {
        var autoMocker = new AutoMocker();

        var mockMessageHandler = new Mock<IServiceBusMessageHandler>();

        autoMocker.GetMock<IServiceBusHandlerResolver>()
            .Setup(x => x.ResolveHandlers(
                It.Is<string>(subject => subject == "subject1")
            ))
            .Returns([mockMessageHandler.Object]);

        var processingService = autoMocker.CreateInstance<ServiceBusProcessingService>();

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var fakeMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(subject: "subject1");
        var fakeArgs = new ProcessMessageEventArgs(fakeMessage, null!, null!, cancellationTokenSource.Token);

        await processingService.OnProcessMessageAsync(fakeArgs);

        mockMessageHandler.Verify(x =>
            x.HandleAsync(
                It.IsAny<ServiceBusReceivedMessage>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    #endregion

    #region OnProcessErrorAsync(ProcessErrorEventArgs)

    [TestMethod]
    public async Task OnProcessErrorAsync_LogsError()
    {
        var autoMocker = new AutoMocker();

        var capturedLogs = new List<string>();
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<ServiceBusProcessingService>(capturedLogs.Add);

        autoMocker.Use(mockLogger.Object);

        var processingService = autoMocker.CreateInstance<ServiceBusProcessingService>();

        var fakeArgs = new ProcessErrorEventArgs(
            exception: new Exception(),
            errorSource: ServiceBusErrorSource.Receive,
            fullyQualifiedNamespace: "fake.namespace",
            entityPath: "entity.path",
            cancellationToken: CancellationToken.None
        );

        await processingService.OnProcessErrorAsync(fakeArgs);

        Assert.HasCount(1, capturedLogs);
        Assert.Contains("Error: Service Bus error in entity entity.path, operation Receive", capturedLogs);
    }

    #endregion
}
