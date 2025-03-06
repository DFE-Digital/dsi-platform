using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Extensions for adding interactor services to a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds discovered interactors into a service collection.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="descriptors">An enumerable collection of interactor type descriptors.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="descriptors"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractors(
        this IServiceCollection services,
        IEnumerable<InteractorTypeDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(descriptors, nameof(descriptors));

        foreach (var descriptor in descriptors) {
            services.AddSingleton(descriptor.ContractType, descriptor.ConcreteType);
        }

        return services;
    }

    /// <summary>
    /// Decorates interactors with request and response model validation.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="setupAction"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractorModelValidation(
        this IServiceCollection services,
        Action<InteractorModelValidationOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        services.AddOptions();
        services.Configure(setupAction);

        services.Decorate(typeof(IInteractor<,>), typeof(InteractorModelValidator<,>));

        return services;
    }
}
