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

    #region DispatchAsync<TRequest>(IInteractionDispatcher, TRequest)

    [TestMethod]
    public async Task DispatchAsync_Request_Throws_WhenInteractionArgumentIsNull()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionDispatcherExtensions.DispatchAsync(
                null!,
                FakeRequest
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
                nullRequest
            ).To<ExampleResponse>());
    }

    [TestMethod]
    public async Task DispatchAsync_Request_CallsDispatchAsyncWithInteractionContext()
    {
        var mockInteraction = new Mock<IInteractionDispatcher>();

        await InteractionDispatcherExtensions.DispatchAsync(
            mockInteraction.Object,
            FakeRequest
        );

        mockInteraction.Verify(x =>
            x.DispatchAsync(
                It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest))
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
                    It.Is<InteractionContext<ExampleRequest>>(ctx => ReferenceEquals(ctx.Request, FakeRequest))
                )
            )
            .Returns(InteractionTask.FromResult(FakeResponse));

        var response = await InteractionDispatcherExtensions.DispatchAsync(
            mockInteraction.Object,
            FakeRequest
        );

        Assert.AreSame(FakeResponse, response);
    }

    #endregion
}
