using System.Reflection;
using Dfe.SignIn.Core.Framework.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework.Tests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region AddInteractors(IServiceCollection, Assembly)

    [TestMethod]
    public void AddInteractors_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ServiceCollectionExtensions.AddInteractors(null, [])
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddInteractors_Throws_WhenAssemblyArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddInteractors(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddInteractors_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddInteractors(
            InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(TestAssembly)
        );

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(ExampleUseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExampleUseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(ExampleApiRequester)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExampleApiRequester)
            )
        );
    }

    #endregion
}
