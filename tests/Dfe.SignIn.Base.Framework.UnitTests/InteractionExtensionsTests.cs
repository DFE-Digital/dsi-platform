using System.Reflection;
using Dfe.SignIn.Base.Framework.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractionExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region AddInteractionFramework(IServiceCollection)

    [TestMethod]
    public void AddInteractionFramework_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionExtensions.AddInteractionFramework(
                services: null!
            ));
    }

    [TestMethod]
    public void AddInteractionFramework_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        InteractionExtensions.AddInteractionFramework(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionValidator) &&
                descriptor.ImplementationType == typeof(InteractionValidator)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractionDispatcher) &&
                descriptor.ImplementationType == typeof(ServiceProviderInteractionDispatcher)
            )
        );
    }

    [TestMethod]
    public void AddInteractionFramework_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = InteractionExtensions.AddInteractionFramework(services);

        Assert.AreSame(services, result);
    }

    #endregion

    #region AddInteractor<TConcreteInteractor>(IServiceCollection)

    [TestMethod]
    public void AddInteractor_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionExtensions.AddInteractor<ExampleUseCase>(
                services: null!
            ));
    }

    [TestMethod]
    public void AddInteractor_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        InteractionExtensions.AddInteractor<ExampleUseCase>(services);

        Assert.IsTrue(
            services.HasInteractor<ExampleRequest, ExampleUseCase>()
        );
    }

    [TestMethod]
    public void AddInteractor_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = InteractionExtensions.AddInteractor<ExampleUseCase>(services);

        Assert.AreSame(services, result);
    }

    #endregion

    #region AddInteractors(IServiceCollection, Assembly)

    [TestMethod]
    public void AddInteractors_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionExtensions.AddInteractors(
                services: null!,
                descriptors: []
            ));
    }

    [TestMethod]
    public void AddInteractors_Throws_WhenDescriptorsArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractionExtensions.AddInteractors(
                services,
                descriptors: null!
            ));
    }

    [TestMethod]
    public void AddInteractors_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        InteractionExtensions.AddInteractors(
            services,
            InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(TestAssembly)
        );

        Assert.IsTrue(
            services.HasInteractor<ExampleRequest, ExampleUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<AnotherExampleRequest, AnotherExampleUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<ExampleRequest, Example_ApiRequester>()
        );
        Assert.IsTrue(
            services.HasInteractor<AnotherExampleRequest, AnotherExample_ApiRequester>()
        );
    }

    [TestMethod]
    public void AddInteractors_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = InteractionExtensions.AddInteractors(services, []);

        Assert.AreSame(services, result);
    }

    #endregion
}
