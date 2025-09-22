using Dfe.SignIn.Base.Framework.Caching;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests.Caching;

[TestClass]
public sealed class CachedInteractorTests
{
    public sealed record ExampleRequest : ICacheableRequest
    {
        public required string CacheKey { get; init; }
    }

    public sealed record ExampleResponse
    {
    }

    private static readonly ExampleRequest FakeRequest = new() { CacheKey = "abc" };

    [TestMethod]
    public async Task DoesNotInvokeInnerInteractor_WhenUsingCachedResponse()
    {
        var autoMocker = new AutoMocker();
        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var cancellationToken = new CancellationToken();

        var fakeCachedResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractionCache<ExampleRequest>>()
            .Setup(x => x.GetAsync(
                It.Is<ExampleRequest>(p => ReferenceEquals(FakeRequest, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeCachedResponse);

        await cachedInteractor.InvokeAsync(FakeRequest);

        autoMocker.Verify<IInteractor<ExampleRequest>>(x =>
            x.InvokeAsync(
                It.IsAny<InteractionContext<ExampleRequest>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task ReturnsCachedResponse()
    {
        var autoMocker = new AutoMocker();
        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var cancellationToken = new CancellationToken();

        var fakeCachedResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractionCache<ExampleRequest>>()
            .Setup(x => x.GetAsync(
                It.Is<ExampleRequest>(p => ReferenceEquals(FakeRequest, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeCachedResponse);

        var response = await cachedInteractor.InvokeAsync(FakeRequest);

        Assert.AreSame(fakeCachedResponse, response);
    }

    [TestMethod]
    public async Task InvokesInnerInteractor_WhenCacheMiss()
    {
        var autoMocker = new AutoMocker();
        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var context = new InteractionContext<ExampleRequest>(FakeRequest);
        var cancellationToken = new CancellationToken();

        await cachedInteractor.InvokeAsync(context, cancellationToken);

        autoMocker.Verify<IInteractor<ExampleRequest>>(x =>
            x.InvokeAsync(
                It.Is<InteractionContext<ExampleRequest>>(p => ReferenceEquals(context, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ReturnsInnerResponse_WhenCacheMiss()
    {
        var autoMocker = new AutoMocker();

        var context = new InteractionContext<ExampleRequest>(FakeRequest);
        var cancellationToken = new CancellationToken();

        var fakeResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractor<ExampleRequest>>()
            .Setup(x => x.InvokeAsync(
                It.Is<InteractionContext<ExampleRequest>>(p => ReferenceEquals(context, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeResponse);

        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var response = await cachedInteractor.InvokeAsync(context, cancellationToken);

        Assert.AreSame(fakeResponse, response);
    }

    [TestMethod]
    public async Task InvokesInnerInteractor_WhenCacheIgnoreHintProvided()
    {
        var autoMocker = new AutoMocker();
        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var context = new InteractionContext<ExampleRequest>(FakeRequest);
        var cancellationToken = new CancellationToken();

        var fakeCachedResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractionCache<ExampleRequest>>()
            .Setup(x => x.GetAsync(
                It.Is<ExampleRequest>(p => ReferenceEquals(FakeRequest, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeCachedResponse);

        context.IgnoreCacheHint = true;
        await cachedInteractor.InvokeAsync(context, cancellationToken);

        autoMocker.Verify<IInteractor<ExampleRequest>>(x =>
            x.InvokeAsync(
                It.Is<InteractionContext<ExampleRequest>>(p => ReferenceEquals(context, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task ReturnsInnerResponse_WhenCacheIgnoreHintProvided()
    {
        var autoMocker = new AutoMocker();
        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        var context = new InteractionContext<ExampleRequest>(FakeRequest);
        var cancellationToken = new CancellationToken();

        var fakeResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractor<ExampleRequest>>()
            .Setup(x => x.InvokeAsync(
                It.Is<InteractionContext<ExampleRequest>>(p => ReferenceEquals(context, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeResponse);

        var fakeCachedResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractionCache<ExampleRequest>>()
            .Setup(x => x.GetAsync(
                It.Is<ExampleRequest>(p => ReferenceEquals(FakeRequest, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeCachedResponse);

        context.IgnoreCacheHint = true;
        var response = await cachedInteractor.InvokeAsync(context, cancellationToken);

        Assert.AreSame(fakeResponse, response);
    }

    [TestMethod]
    public async Task SetsCache_WhenCacheMiss()
    {
        var autoMocker = new AutoMocker();

        var context = new InteractionContext<ExampleRequest>(FakeRequest);
        var cancellationToken = new CancellationToken();

        var fakeResponse = new ExampleResponse();
        autoMocker.GetMock<IInteractor<ExampleRequest>>()
            .Setup(x => x.InvokeAsync(
                It.Is<InteractionContext<ExampleRequest>>(p => ReferenceEquals(context, p)),
                It.Is<CancellationToken>(p => Equals(cancellationToken, p))
            ))
            .ReturnsAsync(fakeResponse);

        var cachedInteractor = autoMocker.CreateInstance<CachedInteractor<ExampleRequest>>();

        await cachedInteractor.InvokeAsync(context, cancellationToken);

        autoMocker.Verify<IInteractionCache<ExampleRequest>>(x =>
            x.SetAsync(
                It.Is<ExampleRequest>(p => ReferenceEquals(FakeRequest, p)),
                It.Is<object>(p => ReferenceEquals(fakeResponse, p))
            ),
            Times.Once
        );
    }
}
