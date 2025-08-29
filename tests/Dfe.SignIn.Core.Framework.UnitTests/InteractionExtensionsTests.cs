using System.Reflection;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class InteractionExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region AddInteractionFramework(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractionFramework_Throws_WhenServicesArgumentIsNull()
    {
        InteractionExtensions.AddInteractionFramework(
            services: null!
        );
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractor_Throws_WhenServicesArgumentIsNull()
    {
        InteractionExtensions.AddInteractor<ExampleUseCase>(
            services: null!
        );
    }

    [TestMethod]
    public void AddInteractor_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        InteractionExtensions.AddInteractor<ExampleUseCase>(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest>) &&
                descriptor.ImplementationType == typeof(ExampleUseCase)
            )
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractors_Throws_WhenServicesArgumentIsNull()
    {
        InteractionExtensions.AddInteractors(
            services: null!,
            descriptors: []
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractors_Throws_WhenDescriptorsArgumentIsNull()
    {
        var services = new ServiceCollection();

        InteractionExtensions.AddInteractors(
            services,
            descriptors: null!
        );
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
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest>) &&
                descriptor.ImplementationType == typeof(ExampleUseCase)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest>) &&
                descriptor.ImplementationType == typeof(AnotherExampleUseCase)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest>) &&
                descriptor.ImplementationType == typeof(Example_ApiRequester)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest>) &&
                descriptor.ImplementationType == typeof(AnotherExample_ApiRequester)
            )
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
