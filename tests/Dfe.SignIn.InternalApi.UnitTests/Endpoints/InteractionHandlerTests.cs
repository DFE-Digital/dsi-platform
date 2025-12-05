using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Diagnostics;
using Dfe.SignIn.InternalApi.Contracts;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.InternalApi.UnitTests.Endpoints;

[TestClass]
public sealed class InteractionHandlerTests
{
    private static Mock<HttpContext> CreateMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        var mockResponse = autoMocker.GetMock<HttpResponse>();
        mockContext.Setup(x => x.Response)
            .Returns(mockResponse.Object);

        return mockContext;
    }

    #region InteractionHandler<TRequest, TResponse>(TRequest, ...)

    [TestMethod]
    public async Task InteractionHandler_DispatchesExpectedRequest()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        PingRequest? capturedRequest = null;
        autoMocker.CaptureInteractionContext<PingRequest>(ctx => {
            capturedRequest = ctx.Request;
        });

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };

        await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        Assert.AreSame(fakeRequest, capturedRequest);
    }

    [TestMethod]
    public async Task InteractionHandler_ReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };
        var fakeResponse = new PingResponse { Value = 123 };
        autoMocker.MockResponse(fakeRequest, fakeResponse);

        var result = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        autoMocker.GetMock<HttpResponse>().VerifySet(x => x.StatusCode = It.IsAny<int>(), Times.Never);

        var interactionResponse = TypeAssert.IsType<InteractionResponse<PingResponse>>(result);
        Assert.AreEqual("Dfe.SignIn.Core.Contracts.Diagnostics.PingResponse", interactionResponse.Type);
        Assert.AreEqual(123, interactionResponse.Data.Value);
    }

    [TestMethod]
    public async Task InteractionHandler_Throws_WhenOperationIsCancelled()
    {
        var autoMocker = new AutoMocker();
        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();
        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };
        autoMocker.MockThrows(fakeRequest, new OperationCanceledException());

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(()
            => InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
                mockHttpContext.Object,
                fakeRequest,
                // ---
                mockLogger.Object,
                mockExceptionSerializer.Object,
                autoMocker.GetMock<IInteractionDispatcher>().Object
            ));
    }

    [TestMethod]
    public async Task InteractionHandler_ReturnsExpectedException_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();

        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };
        var fakeException = new InvalidRequestException();
        autoMocker.MockThrows(fakeRequest, fakeException);

        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();
        var fakeExceptionJsonElement = JsonDocument.Parse("{}").RootElement;
        mockExceptionSerializer
            .Setup(x => x.SerializeExceptionToJson(
                It.Is<Exception>(ex => ReferenceEquals(fakeException, ex))
            ))
            .Returns(fakeExceptionJsonElement);

        var result = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        autoMocker.GetMock<HttpResponse>().VerifySet(x => x.StatusCode = 400, Times.Once);

        var failedInteractionResponse = TypeAssert.IsType<FailedInteractionResponse>(result);
        Assert.AreEqual(fakeExceptionJsonElement, failedInteractionResponse.Exception);
    }

    [TestMethod]
    public async Task InteractionHandler_LogsError_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();

        var capturedLogs = new List<string>();
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<
            InteractionHandlerExtensions.LoggerContext
        >(capturedLogs.Add);

        var mockExceptionSerializer = autoMocker.GetMock<IExceptionJsonSerializer>();

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };
        autoMocker.MockThrows(fakeRequest, new InvalidRequestException());

        await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        Assert.HasCount(1, capturedLogs);
        Assert.AreEqual("Error: Invalid request.", capturedLogs[0]);
    }

    [TestMethod]
    public async Task InteractionHandler_ReturnsExpectedException_WhenFailureOccurs()
    {
        var autoMocker = new AutoMocker();

        var mockLogger = autoMocker.GetMock<ILogger<InteractionHandlerExtensions.LoggerContext>>();

        var mockHttpContext = CreateMockHttpContext(autoMocker);
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

        var result = await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        autoMocker.GetMock<HttpResponse>().VerifySet(x => x.StatusCode = 500, Times.Once);

        var failedInteractionResponse = TypeAssert.IsType<FailedInteractionResponse>(result);
        Assert.AreEqual(fakeExceptionJsonElement, failedInteractionResponse.Exception);
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

        var mockHttpContext = CreateMockHttpContext(autoMocker);
        var fakeRequest = new PingRequest { Value = 123 };
        autoMocker.MockThrows(fakeRequest, new InvalidOperationException());

        await InteractionHandlerExtensions.InteractionHandler<PingRequest, PingResponse>(
            mockHttpContext.Object,
            fakeRequest,
            // ---
            mockLogger.Object,
            mockExceptionSerializer.Object,
            autoMocker.GetMock<IInteractionDispatcher>().Object
        );

        Assert.HasCount(1, capturedLogs);
        Assert.AreEqual("Error: An error occurred whilst processing internal API request.", capturedLogs[0]);
    }

    #endregion
}
