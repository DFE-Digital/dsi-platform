using Moq;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractionDispatcherExtensionsTests
{
    private sealed record ExampleRequest
    {
    }

    private sealed record ExampleResponse
    {
    }

    private static readonly ExampleRequest FakeRequest = new();
    private static readonly ExampleResponse FakeResponse = new();
    private static readonly CancellationToken FakeCancellationToken = new();

    #region DispatchAsync<TRequest>(IInteractionDispatcher, TRequest, CancellationToken)

    [TestMethod]
    public async Task DispatchAsync_Request_Throws_WhenInteractionArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchAsync(
                null!,
                FakeRequest,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchAsync_Request_Throws_WhenRequestArgumentIsNull()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        ExampleRequest nullRequest = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchAsync(
                mockInteraction.Object,
                nullRequest,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchAsync_Request_CallsDispatchAsyncWithInteractionContext()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        await InteractionDispatcherExtensions.DispatchAsync(
            mockInteraction.Object,
            FakeRequest,
            FakeCancellationToken
        );

        mockInteraction.Verify(x =>
            x.DispatchAsync(
                It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task DispatchAsync_Request_ReturnsExpectedResponse()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        mockInteraction
            .Setup(x =>
                x.DispatchAsync(
                    It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                    It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
                )
            )
            .Returns(InteractionTask.FromResult(FakeResponse));

        var response = await InteractionDispatcherExtensions.DispatchAsync(
            mockInteraction.Object,
            FakeRequest,
            FakeCancellationToken
        );

        Assert.AreSame(FakeResponse, response);
    }

    #endregion

    #region DispatchIgnoreCacheAsync<TRequest>(IInteractionDispatcher, InteractionContext<TRequest>, CancellationToken)

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_InteractionContext_Throws_WhenInteractionArgumentIsNull()
    {
        var interactionContext = new InteractionContext<ExampleRequest>(FakeRequest);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
                null!,
                interactionContext,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_InteractionContext_Throws_WhenContextArgumentIsNull()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        InteractionContext<ExampleRequest> nullInteractionContext = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
                mockInteraction.Object,
                nullInteractionContext,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_InteractionContext_SetsIgnoreCacheHint()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleRequest>(FakeRequest);

        await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            interactionContext,
            FakeCancellationToken
        );

        Assert.IsTrue(interactionContext.IgnoreCacheHint);
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_InteractionContext_CallsDispatchAsyncWithInteractionContext()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleRequest>(FakeRequest);

        await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            interactionContext,
            FakeCancellationToken
        );

        mockInteraction.Verify(x =>
            x.DispatchAsync(
                It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_InteractionContext_ReturnsExpectedResponse()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        var interactionContext = new InteractionContext<ExampleRequest>(FakeRequest);

        mockInteraction
            .Setup(x =>
                x.DispatchAsync(
                    It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                    It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
                )
            )
            .Returns(InteractionTask.FromResult(FakeResponse));

        var response = await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            interactionContext,
            FakeCancellationToken
        );

        Assert.AreSame(FakeResponse, response);
    }

    #endregion

    #region DispatchIgnoreCacheAsync<TRequest>(IInteractionDispatcher, TRequest, CancellationToken)

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_Request_Throws_WhenInteractionArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
                null!,
                FakeRequest,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_Request_Throws_WhenContextArgumentIsNull()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();
        ExampleRequest nullRequest = null!;

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
                mockInteraction.Object,
                nullRequest,
                FakeCancellationToken
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_Request_SetsIgnoreCacheHint()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            FakeRequest,
            FakeCancellationToken
        );

        mockInteraction.Verify(x =>
            x.DispatchAsync(
                It.Is<InteractionContext<ExampleRequest>>(ctx => ctx.IgnoreCacheHint),
                It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_Request_CallsDispatchAsyncWithInteractionContext()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            FakeRequest,
            FakeCancellationToken
        );

        mockInteraction.Verify(x =>
            x.DispatchAsync(
                It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task DispatchIgnoreCacheAsync_Request_ReturnsExpectedResponse()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        mockInteraction
            .Setup(x =>
                x.DispatchAsync(
                    It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest)),
                    It.Is<CancellationToken>(p => Equals(p, FakeCancellationToken))
                )
            )
            .Returns(InteractionTask.FromResult(FakeResponse));

        var response = await InteractionDispatcherExtensions.DispatchIgnoreCacheAsync(
            mockInteraction.Object,
            FakeRequest,
            FakeCancellationToken
        );

        Assert.AreSame(FakeResponse, response);
    }

    #endregion
}
