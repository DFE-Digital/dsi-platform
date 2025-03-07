using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dfe.SignIn.Core.Framework.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework.Tests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region AddInteractor<TConcreteInteractor>(IServiceCollection)

    [TestMethod]
    public void AddInteractor_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ServiceCollectionExtensions.AddInteractor<Example_UseCaseHandler>(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddInteractor_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddInteractor<Example_UseCaseHandler>();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(Example_UseCaseHandler)
            )
        );
    }

    #endregion

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
                descriptor.ImplementationType == typeof(Example_UseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExample_UseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(Example_ApiRequester)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExample_ApiRequester)
            )
        );
    }

    #endregion

    #region AddInteractorModelValidation(IServiceCollection)

    [TestMethod]
    public void AddInteractorModelValidation_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ServiceCollectionExtensions.AddInteractorModelValidation(null, options => { })
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void AddInteractorModelValidation_Throws_WhenSetupActionArgumentIsNull()
    {
        var services = new ServiceCollection();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => services.AddInteractorModelValidation(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public async Task AddInteractorModelValidation_DecoratesInteractorWithValidator()
    {
        var services = new ServiceCollection();

        services.AddSingleton<
            IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>,
            ExampleInteractorWithValidation_ApiRequester
        >();

        services.AddInteractorModelValidation(options => { });

        var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
        var interactor = provider.GetRequiredService<
            IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        Assert.IsNotNull(interactor);

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => interactor.InvokeAsync(new ExampleInteractorWithValidationRequest {
                Name = "A",
            })
        );
    }

    #endregion
}
