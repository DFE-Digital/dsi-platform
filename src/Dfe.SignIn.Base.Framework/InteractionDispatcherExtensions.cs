namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Provides extension methods for the <see cref="IInteractionDispatcher"/>
/// interface, enabling a fluent API for building and dispatching interactions.
/// </summary>
public static class InteractionDispatcherExtensions
{
    /// <summary>
    /// Creates a fluent builder for configuring and dispatching an interaction.
    /// </summary>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance
    /// used to dispatch interactions.</param>
    /// <returns>
    ///   <para>An <see cref="IInteractionContextBuilder"/> that allows configuring
    ///   options such as cancellation tokens and cache hints before dispatching the
    ///   interaction.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    /// </exception>
    public static IInteractionContextBuilder Build(this IInteractionDispatcher interaction)
        => new InteractionContextBuilder(interaction);

    /// <inheritdoc cref="IInteractionDispatcher.DispatchAsync"/>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance.</param>
    /// <param name="request">Request model of the interaction.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    public static InteractionTask DispatchAsync<TRequest>(
        this IInteractionDispatcher interaction,
        TRequest request)
        where TRequest : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var context = new InteractionContext<TRequest>(request);
        return interaction.DispatchAsync(context);
    }
}

/// <summary>
/// Defines a fluent builder for configuring and dispatching interactions.
/// </summary>
public interface IInteractionContextBuilder
{
    /// <summary>
    /// Overrides the default <see cref="CancellationToken"/> used for interactions.
    /// </summary>
    /// <remarks>
    ///   <para>By default, the token is obtained from the active request via the
    ///   <see cref="ICancellationContext"/> service. This method allows you to
    ///   replace that token with a custom one for the current interaction.</para>
    ///   <para>This can be useful when you want to prevent cancellation since
    ///   <see cref="CancellationToken.None"/> can be specified:</para>
    ///   <code language="csharp"><![CDATA[
    ///     var response = await interaction.Build()
    ///         .WithCancellation(CancellationToken.None)
    ///         .DispatchAsync(new ExampleRequest())
    ///         .To<ExampleResponse>();
    ///   ]]></code>
    ///   <para>Use this when you need to control cancellation independently of
    ///   the active request context, for example, when dispatching an interaction
    ///   as part of a background operation or a composite workflow.</para>
    ///   <para>The <see cref="CancellationToken"/> cannot be overridden for request
    ///   types that are annotated with <see cref="NonCancellableAttribute"/>.</para>
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token that is to be
    /// observed during dispatch.</param>
    /// <returns>
    ///   <para>The <see cref="IInteractionContextBuilder"/> instance for chained calls.</para>
    /// </returns>
    IInteractionContextBuilder WithCancellation(CancellationToken cancellationToken);

    /// <summary>
    /// Indicates whether the interaction should ignore any cached results where
    /// possible. It is not guaranteed that caching will be ignored.
    /// </summary>
    /// <param name="ignoreHint">A value hinting whether cached responses should
    /// be ignored.</param>
    /// <returns>
    ///   <para>The <see cref="IInteractionContextBuilder"/> instance for chained calls.</para>
    /// </returns>
    IInteractionContextBuilder IgnoreCache(bool ignoreHint = true);

    /// <inheritdoc cref="IInteractionDispatcher.DispatchAsync"/>
    /// <param name="request">Request model of the interaction.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    InteractionTask DispatchAsync<TRequest>(TRequest request)
        where TRequest : class;
}

/// <summary>
/// Default implementation of <see cref="IInteractionContextBuilder"/> for
/// configuring and dispatching interactions.
/// </summary>
public sealed class InteractionContextBuilder : IInteractionContextBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionContextBuilder"/> class.
    /// </summary>
    /// <param name="interaction">The <see cref="IInteractionDispatcher"/> instance
    /// used to dispatch interactions.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="interaction"/> is null.</para>
    /// </exception>
    public InteractionContextBuilder(IInteractionDispatcher interaction)
    {
        ExceptionHelpers.ThrowIfArgumentNull(interaction, nameof(interaction));

        this.Interaction = interaction;
    }

    private readonly IInteractionDispatcher Interaction;
    private CancellationToken? CancellationToken = null;
    private bool IgnoreCacheHint = false;

    /// <inheritdoc/>
    public IInteractionContextBuilder WithCancellation(CancellationToken cancellationToken)
    {
        this.CancellationToken = cancellationToken;
        return this;
    }

    /// <inheritdoc/>
    public IInteractionContextBuilder IgnoreCache(bool ignoreHint = true)
    {
        this.IgnoreCacheHint = ignoreHint;
        return this;
    }

    /// <inheritdoc/>
    public InteractionTask DispatchAsync<TRequest>(TRequest request)
        where TRequest : class
    {
        return this.Interaction.DispatchAsync(
            new InteractionContext<TRequest>(request) {
                CancellationToken = this.CancellationToken,
                IgnoreCacheHint = this.IgnoreCacheHint,
            }
        );
    }
}
