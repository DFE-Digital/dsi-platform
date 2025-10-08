namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Extension methods for the <see cref="IInteractionDispatcher"/> interface.
/// </summary>
public static class InteractionDispatcherExtensions
{
    /// <inheritdoc cref="IInteractionDispatcher.DispatchAsync"/>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance.</param>
    /// <param name="request">Request model of the interaction.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    public static InteractionTask DispatchAsync<TRequest>(
        this IInteractionDispatcher interaction,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var context = new InteractionContext<TRequest>(request);
        return interaction.DispatchAsync(context, cancellationToken);
    }

    /// <summary>
    /// Dispatches an interaction request and awaits a response and attempts to ignore
    /// the use of caching on the interaction request.
    /// </summary>
    /// <inheritdoc cref="IInteractionDispatcher.DispatchAsync"/>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance.</param>
    /// <param name="context">Context of the interaction.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    public static InteractionTask DispatchIgnoreCacheAsync<TRequest>(
        this IInteractionDispatcher interaction,
        InteractionContext<TRequest> context,
        CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        context.IgnoreCacheHint = true;
        return interaction.DispatchAsync(context, cancellationToken);
    }

    /// <summary>
    /// Dispatches an interaction request and awaits a response and attempts to ignore
    /// the use of caching on the interaction request.
    /// </summary>
    /// <inheritdoc cref="IInteractionDispatcher.DispatchAsync"/>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance.</param>
    /// <param name="request">Request model of the interaction.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    public static InteractionTask DispatchIgnoreCacheAsync<TRequest>(
        this IInteractionDispatcher interaction,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var context = new InteractionContext<TRequest>(request);
        return DispatchIgnoreCacheAsync(interaction, context, cancellationToken);
    }
}
