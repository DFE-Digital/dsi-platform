namespace Dfe.SignIn.Base.Framework.Caching;

/// <summary>
/// A decorator for <see cref="IInteractor{TRequest}"/> that adds caching capabilities.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request being handled, which must implement <see cref="IKeyedRequest"/>.
/// </typeparam>
/// <param name="inner">The underlying interactor to be decorated with caching.</param>
/// <param name="cache">The cache implementation used to store and retrieve interaction responses.</param>
public sealed class CachedInteractor<TRequest>(
    IInteractor<TRequest> inner,
    IInteractionCache<TRequest> cache
) : IInteractor<TRequest>
    where TRequest : class, IKeyedRequest
{
    /// <inheritdoc/>
    public async Task<object> InvokeAsync(
        InteractionContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        if (!context.IgnoreCacheHint) {
            var cachedResponse = await cache.GetAsync(context.Request, cancellationToken);
            if (cachedResponse is not null) {
                return cachedResponse;
            }
        }

        var response = await inner.InvokeAsync(context, cancellationToken);
        await cache.SetAsync(context.Request, response);
        return response;
    }
}
