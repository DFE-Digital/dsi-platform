using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractionContextBuilderTests
{
    public sealed record ExampleRequest { }

    public sealed record ExampleResponse
    {
        public int Value { get; init; }
    }

    #region InteractionContextBuilder(IInteractionDispatcher)

    [TestMethod]
    public void InteractionContextBuilder_Throws_WhenInteractionArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => new InteractionContextBuilder(
                interaction: null!
            ));
    }

    [TestMethod]
    public void InteractionContextBuilder_SetsExpectedDefaults()
    {
        var autoMocker = new AutoMocker();

        InteractionContext<ExampleRequest>? capturedInteractionContext = null;
        autoMocker.CaptureInteractionContext<ExampleRequest>(ctx => capturedInteractionContext = ctx);

        var builder = autoMocker.CreateInstance<InteractionContextBuilder>();
        builder.DispatchAsync(new ExampleRequest());

        Assert.IsNotNull(capturedInteractionContext);
        Assert.IsNull(capturedInteractionContext.CancellationToken);
        Assert.IsFalse(capturedInteractionContext.IgnoreCacheHint);
    }

    #endregion

    #region WithCancellation(CancellationToken)

    [TestMethod]
    public void WithCancellation_OverridesCancellationToken()
    {
        var autoMocker = new AutoMocker();

        using var cancellationSource = new CancellationTokenSource();

        InteractionContext<ExampleRequest>? capturedInteractionContext = null;
        autoMocker.CaptureInteractionContext<ExampleRequest>(ctx => capturedInteractionContext = ctx);
        var builder = autoMocker.CreateInstance<InteractionContextBuilder>();

        builder
            .WithCancellation(cancellationSource.Token)
            .DispatchAsync(new ExampleRequest());

        // Ensure that correct cancellation token was set in context.
        Assert.IsNotNull(capturedInteractionContext);
        Assert.AreEqual(cancellationSource.Token, capturedInteractionContext.CancellationToken);
    }

    #endregion

    #region IgnoreCache(bool)

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IgnoreCache_OverridesIgnoreCacheHint(bool ignoreHint)
    {
        var autoMocker = new AutoMocker();

        InteractionContext<ExampleRequest>? capturedInteractionContext = null;
        autoMocker.CaptureInteractionContext<ExampleRequest>(ctx => capturedInteractionContext = ctx);

        var builder = autoMocker.CreateInstance<InteractionContextBuilder>();
        builder
            .IgnoreCache(ignoreHint)
            .DispatchAsync(new ExampleRequest());

        Assert.IsNotNull(capturedInteractionContext);
        Assert.AreEqual(ignoreHint, capturedInteractionContext.IgnoreCacheHint);
    }

    #endregion

    #region DispatchAsync<TRequest>(TRequest)

    [TestMethod]
    public async Task DispatchAsync_ReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new ExampleRequest(),
            new ExampleResponse { Value = 42 }
        );

        var builder = autoMocker.CreateInstance<InteractionContextBuilder>();
        var response = await builder
            .DispatchAsync(new ExampleRequest())
            .To<ExampleResponse>();

        Assert.AreEqual(42, response.Value);
    }

    #endregion
}
