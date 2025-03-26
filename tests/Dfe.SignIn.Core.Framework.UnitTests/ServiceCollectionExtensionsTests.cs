using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region AddInteractor<TConcreteInteractor>(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractor_Throws_WhenServicesArgumentIsNull()
    {
        ServiceCollectionExtensions.AddInteractor<Example_UseCaseHandler>(
            services: null!
        );
    }

    [TestMethod]
    public void AddInteractor_RegistersExpectedServices()
    {
        var services = new ServiceCollection();

        services.AddInteractor<Example_UseCaseHandler>();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(Example_UseCaseHandler)
            )
        );
    }

    #endregion

    #region AddInteractors(IServiceCollection, Assembly)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractors_Throws_WhenServicesArgumentIsNull()
    {
        ServiceCollectionExtensions.AddInteractors(
            services: null!,
            descriptors: []
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractors_Throws_WhenDescriptorsArgumentIsNull()
    {
        var services = new ServiceCollection();

        services.AddInteractors(
            descriptors: null!
        );
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
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(Example_UseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExample_UseCaseHandler)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<ExampleRequest, ExampleResponse>) &&
                descriptor.ImplementationType == typeof(Example_ApiRequester)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>) &&
                descriptor.ImplementationType == typeof(AnotherExample_ApiRequester)
            )
        );
    }

    #endregion

    #region AddInteractorModelValidation(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractorModelValidation_Throws_WhenServicesArgumentIsNull()
    {
        ServiceCollectionExtensions.AddInteractorModelValidation(
            services: null!,
            setupAction: options => { }
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddInteractorModelValidation_Throws_WhenSetupActionArgumentIsNull()
    {
        var services = new ServiceCollection();

        services.AddInteractorModelValidation(
            setupAction: null!
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public async Task AddInteractorModelValidation_DecoratesInteractorWithValidator()
    {
        var services = new ServiceCollection();

        services.AddTransient<
            IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>,
            ExampleInteractorWithValidation_ApiRequester
        >();

        services.AddInteractorModelValidation(options => { });

        var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
        var interactor = provider.GetRequiredService<
            IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
        >();

        Assert.IsNotNull(interactor);

        await interactor.InvokeAsync(new ExampleInteractorWithValidationRequest {
            Name = "A",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        });
    }

    #endregion
}
