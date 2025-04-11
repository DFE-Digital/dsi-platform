using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Extensions for adding interactor services to a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a concrete interactor implementation into a service collection.
    /// </summary>
    /// <typeparam name="TConcreteInteractor">The type of interactor.</typeparam>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractor<TConcreteInteractor>(
        this IServiceCollection services)
        where TConcreteInteractor : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        var contractType = typeof(TConcreteInteractor).GetInterfaces()
            .First(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IInteractor<,>));

        services.AddTransient(contractType, typeof(TConcreteInteractor));

        return services;
    }

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
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(descriptors, nameof(descriptors));

        foreach (var descriptor in descriptors) {
            services.AddTransient(descriptor.ContractType, descriptor.ConcreteType);
        }

        return services;
    }

    /// <summary>
    /// Decorates interactors with request and response model validation.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <param name="setupAction">Action to setup options.</param>
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
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(setupAction, nameof(setupAction));

        services.AddOptions();
        services.Configure(setupAction);

        services.Decorate(typeof(IInteractor<,>), typeof(InteractorModelValidator<,>));

        return services;
    }
}
