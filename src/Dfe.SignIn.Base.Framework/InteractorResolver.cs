using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Represents a service that can resolve interactor implementations.
/// </summary>
public interface IInteractorResolver
{
    /// <summary>
    /// Resolves an interactor implementation for a given request type.
    /// </summary>
    /// <typeparam name="TRequest">The type of request.</typeparam>
    /// <returns>
    ///   <para>An interactor instance when one could be resolved; otherwise, null.</para>
    /// </returns>
    IInteractor<TRequest>? ResolveInteractor<TRequest>() where TRequest : class;
}

/// <summary>
/// A concrete implementation of <see cref="IInteractorResolver"/> that resolves
/// interactor implementations from a <see cref="IServiceProvider"/>.
/// </summary>
/// <param name="provider">The service provider.</param>
public sealed class ServiceProviderInteractorResolver(IServiceProvider provider) : IInteractorResolver
{
    /// <inheritdoc/>
    public IInteractor<TRequest>? ResolveInteractor<TRequest>() where TRequest : class
    {
        return provider.GetService<IInteractor<TRequest>>();
    }
}
