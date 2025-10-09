using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Helper functionality for verifying interaction registrations.
/// </summary>
public static class InteractorTestExtensions
{
    /// <summary>
    /// Checks if service collection has a registration for the specified interactor.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <typeparam name="TInteractor">The type of interactor.</typeparam>
    /// <param name="services">The services collection.</param>
    /// <returns>
    ///   <para>A value of true when a interactor registration is found; otherwise, false.</para>
    /// </returns>
    public static bool HasInteractor<TRequest, TInteractor>(this ServiceCollection services)
        where TRequest : class
        where TInteractor : IInteractor<TRequest>
        => services.Any(descriptor =>
            descriptor.Lifetime == ServiceLifetime.Transient &&
            descriptor.ServiceType == typeof(IInteractor<TRequest>) &&
            descriptor.ImplementationType == typeof(TInteractor)
        );

    /// <summary>
    /// Checks if service collection has a registration.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <param name="services">The services collection.</param>
    /// <returns>
    ///   <para>A value of true when a interactor registration is found; otherwise, false.</para>
    /// </returns>
    public static bool HasInteractor<TRequest>(this ServiceCollection services)
        where TRequest : class
        => services.Any(descriptor =>
            descriptor.Lifetime == ServiceLifetime.Transient &&
            descriptor.ServiceType == typeof(IInteractor<TRequest>)
        );
}
