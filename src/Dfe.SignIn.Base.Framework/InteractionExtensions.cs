using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Extensions for adding interactor services to a service collection.
/// </summary>
public static class InteractionExtensions
{
    /// <summary>
    /// Adds services that are required for the interaction framework.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddInteractionFramework(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddOptions();

        services.TryAddSingleton<ICancellationContext, CancellationContext>();
        services.TryAddSingleton<IInteractionValidator, InteractionValidator>();
        services.TryAddSingleton<IInteractionDispatcher, DefaultInteractionDispatcher>();
        services.TryAddSingleton<IInteractorResolver, ServiceProviderInteractorResolver>();

        return services;
    }

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
            .First(interfaceType => interfaceType.GetGenericTypeDefinition() == typeof(IInteractor<>));

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
    /// Adds a null implementation of an interactor into a service collection.
    /// This is useful for testing or when a feature needs to be disabled.
    /// </summary>
    /// <typeparam name="TRequest">The type of request model.</typeparam>
    /// <typeparam name="TResponse">The type of response model.</typeparam>
    /// <param name="services">The collection to add services to.</param>
    /// <returns>
    ///   <para>The <see cref="IServiceCollection"/> so that additional calls can be chained.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddNullInteractor<TRequest, TResponse>(
        this IServiceCollection services)
        where TRequest : class
        where TResponse : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddTransient<IInteractor<TRequest>, NullInteractor<TRequest, TResponse>>();

        return services;
    }
}
