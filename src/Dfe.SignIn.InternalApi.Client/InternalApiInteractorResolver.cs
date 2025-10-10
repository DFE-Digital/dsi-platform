using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// A service that attempts to resolve an interactor instance from the decorated
/// <see cref="IInteractorResolver"/> before assuming an interactor that initiates
/// a request to the internal API.
/// </summary>
/// <param name="inner">The decorated <see cref="IInteractorResolver"/> service.</param>
/// <param name="provider">The service provider.</param>
public sealed class InternalApiInteractorResolver(
    IInteractorResolver inner,
    IServiceProvider provider
) : IInteractorResolver
{
    /// <inheritdoc/>
    public IInteractor<TRequest>? ResolveInteractor<TRequest>() where TRequest : class
    {
        return inner.ResolveInteractor<TRequest>()
            ?? ActivatorUtilities.CreateInstance<InternalApiRequester<TRequest>>(provider);
    }
}
