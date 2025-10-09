using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Diagnostics;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.InternalApi.UnitTests.Endpoints;

[TestClass]
public sealed class InteractionHandlerTests
{
    #region InteractionHandler<TRequest, TResponse>(TRequest, ...)

    [TestMethod]
    public async Task InteractionHandler_DispatchesExpectedRequest()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        PingRequest? capturedRequest = null;
        CancellationToken? capturedCancellationToken = null;
        autoMocker.CaptureRequest<PingRequest>((ctx, cancellationToken) => {
            capturedRequest = ctx.Request;
            capturedCancellationToken = cancellationToken;
        });

        var fakeRequest = new PingRequest { Value = 123 };
        var fakeCancellationTokenSource = new CancellationTokenSource();

        await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object,
            // ---
            fakeCancellationTokenSource.Token
        );

        Assert.AreSame(fakeRequest, capturedRequest);
        Assert.AreEqual(fakeCancellationTokenSource.Token, capturedCancellationToken);
    }

    [TestMethod]
    public async Task InteractionHandler_ReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var fakeRequest = new PingRequest { Value = 123 };
        var fakeResponse = new PingResponse { Value = 123 };
        autoMocker.MockResponse(fakeRequest, fakeResponse);

        var interactionResponse = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object,
            // ---
            CancellationToken.None
        );

        Assert.IsNotNull(interactionResponse);
        Assert.IsNotNull(interactionResponse.Content);
        Assert.AreEqual("Dfe.SignIn.Core.Contracts.Diagnostics.PingResponse", interactionResponse.Content.Type);
        Assert.AreEqual(123, interactionResponse.Content.Data.Value);
        Assert.IsNull(interactionResponse.Exception);
    }

    [TestMethod]
    public async Task InteractionHandler_Throws_WhenOperationIsCancelled()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var fakeRequest = new PingRequest { Value = 123 };
        autoMocker.MockThrows(fakeRequest, new OperationCanceledException());

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(()
            => InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
                fakeRequest,
                // ---
                mockLogger.Object,
                mockExceptionSerializer.Object,
                autoMocker.GetMock<IInteractionDispatcher>().Object,
                // ---
                CancellationToken.None
            ));
    }

    [TestMethod]
    public async Task InteractionHandler_ReturnsExpectedException_WhenFailureOccurs()
    {
        var autoMocker = new AutoMocker();

        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();

        var fakeRequest = new PingRequest { Value = 123 };
        var fakeException = new InvalidOperationException("Fake exception.");
        autoMocker.MockThrows(fakeRequest, fakeException);

        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();
        var fakeExceptionJsonElement = JsonDocument.Parse("{}").RootElement;
        mockExceptionSerializer
            .Setup(x => x.SerializeExceptionToJson(
                It.Is<Exception>(ex => ReferenceEquals(fakeException, ex))
            ))
            .Returns(fakeExceptionJsonElement);

        var interactionResponse = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object,
            // ---
            CancellationToken.None
        );

        Assert.IsNotNull(interactionResponse);
        Assert.IsNotNull(interactionResponse.Exception);
        Assert.AreEqual(fakeExceptionJsonElement, interactionResponse.Exception);
        Assert.IsNull(interactionResponse.Content);
    }

    [TestMethod]
    public async Task InteractionHandler_LogsError_WhenFailureOccurs()
    {
        var autoMocker = new AutoMocker();

        var capturedLogs = new List<string>();
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<
            InteractionHandlerExtensions.LoggerContext
        >(capturedLogs.Add);

        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var fakeRequest = new PingRequest { Value = 123 };
        var fakeException = new InvalidOperationException("Fake exception.");
        autoMocker.MockThrows(fakeRequest, fakeException);

        var interactionResponse = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object,
            // ---
            CancellationToken.None
        );

        Assert.HasCount(1, capturedLogs);
        Assert.AreEqual("Error: An error occurred whilst processing internal API request.", capturedLogs[0]);
    }

    #endregion
}
