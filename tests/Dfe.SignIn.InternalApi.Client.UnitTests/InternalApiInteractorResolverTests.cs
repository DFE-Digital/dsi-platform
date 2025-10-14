using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.InternalApi.Client.UnitTests;

[TestClass]
public sealed class InternalApiInteractorResolverTests
{
    private sealed record MockRequest { }
    private sealed class MockInteractor : IInteractor<MockRequest>
    {
        public Task<object> InvokeAsync(InteractionContext<MockRequest> context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>(null!);
        }
    }

    #region ResolveInteractor<TRequest>()

    [TestMethod]
    public void ResolveInteractor_UsesInnerResolution_WhenInnerCanResolve()
    {
        var mockInnerResolver = new Mock<IInteractorResolver>();
        mockInnerResolver
            .Setup(x => x.ResolveInteractor<MockRequest>())
            .Returns(new MockInteractor());

        var provider = new ServiceCollection().BuildServiceProvider();
        var internalApiInteractorResolver = new InternalApiInteractorResolver(mockInnerResolver.Object, provider);

        var resolvedInteractor = internalApiInteractorResolver.ResolveInteractor<MockRequest>();

        Assert.IsNotNull(resolvedInteractor);
        Assert.IsInstanceOfType<MockInteractor>(resolvedInteractor);
    }

    [TestMethod]
    public void ResolveInteractor_CreatesInternalApiRequester_WhenInnerCannotResolve()
    {
        var mockInnerResolver = new Mock<IInteractorResolver>();

        var provider = new ServiceCollection()
            .AddKeyedSingleton(ServiceCollectionExtensions.InternalApiKey, new Mock<HttpClient>().Object)
            .AddSingleton(new Mock<IOptionsMonitor<JsonSerializerOptions>>().Object)
            .AddSingleton(new Mock<IExceptionJsonSerializer>().Object)
            .BuildServiceProvider();

        var internalApiInteractorResolver = new InternalApiInteractorResolver(mockInnerResolver.Object, provider);

        var resolvedInteractor = internalApiInteractorResolver.ResolveInteractor<MockRequest>();

        Assert.IsNotNull(resolvedInteractor);
        Assert.IsInstanceOfType<InternalApiRequester<MockRequest>>(resolvedInteractor);
    }

    #endregion
}
