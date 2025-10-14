using Dfe.SignIn.Base.Framework.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class ServiceProviderInteractorResolverTests
{
    [TestMethod]
    public void ResolveInteractor_ResolvesInteractorFromServiceProvider()
    {
        var fakeInteractor = new ExampleInteractorWithValidation_ApiRequester();

        var provider = new ServiceCollection()
            .AddSingleton<IInteractor<ExampleInteractorWithValidationRequest>>(fakeInteractor)
            .BuildServiceProvider();

        var resolver = new ServiceProviderInteractorResolver(provider);

        var resolvedInteractor = resolver.ResolveInteractor<ExampleInteractorWithValidationRequest>();

        Assert.AreSame(fakeInteractor, resolvedInteractor);
    }
}
