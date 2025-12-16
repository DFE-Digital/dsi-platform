namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Represents a service for limiting interactions based on a request key.
/// </summary>
public interface IInteractionLimiter
{
    /// <summary>
    /// Attempts to limit an action based on the provided keyed request.
    /// </summary>
    /// <param name="request">The request containing a unique key for limiting.</param>
    /// <returns>
    ///   <para>A result indicating whether the action was rejected.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    Task<InteractionLimiterResult> LimitActionAsync(IKeyedRequest request);

    /// <summary>
    /// Reset cache associated with a keyed request so that the next use of
    /// <see cref="LimitActionAsync(IKeyedRequest)"/> is not limited.
    /// </summary>
    /// <param name="request">The request containing a unique key for limiting.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    Task ResetLimitAsync(IKeyedRequest request);
}

/// <summary>
/// Represents the result of an interaction limiting operation.
/// </summary>
public sealed record InteractionLimiterResult
{
    /// <summary>
    /// A value indicating whether the interaction was rejected by the limiter.
    /// </summary>
    public bool WasRejected { get; init; }
}

/// <summary>
/// Provides extension methods for <see cref="IInteractionLimiter"/>.
/// </summary>
public static class InteractionLimiterExtensions
{
    /// <summary>
    /// Attempts to limit an action and throws an exception if the action is rejected.
    /// </summary>
    /// <param name="limiter">The interaction limiter instance.</param>
    /// <param name="request">The keyed request to limit.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="limiter"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="request"/> is null.</para>
    /// </exception>
    /// <exception cref="InteractionRejectedByLimiterException">
    ///   <para>If the interaction is rejected by the limiter.</para>
    /// </exception>
    public static async Task LimitAndThrowAsync(this IInteractionLimiter limiter, IKeyedRequest request)
    {
        ExceptionHelpers.ThrowIfArgumentNull(limiter, nameof(limiter));

        var result = await limiter.LimitActionAsync(request);
        if (result.WasRejected) {
            throw new InteractionRejectedByLimiterException(request.GetType().Name, request.Key);
        }
    }
}

/// <summary>
/// The exception thrown when an interaction is rejected by an <see cref="IInteractionLimiter"/>.
/// </summary>
public sealed class InteractionRejectedByLimiterException : InteractionException
{
    /// <inheritdoc/>
    public InteractionRejectedByLimiterException() { }

    /// <inheritdoc/>
    public InteractionRejectedByLimiterException(string? message) : base(message) { }

    /// <inheritdoc/>
    public InteractionRejectedByLimiterException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionRejectedByLimiterException"/> class.
    /// </summary>
    /// <param name="requestTypeName">The type name of the request.</param>
    /// <param name="requestKey">The unique key associated with the request.</param>
    public InteractionRejectedByLimiterException(string requestTypeName, string requestKey)
    {
        this.RequestTypeName = requestTypeName;
        this.RequestKey = requestKey;
    }

    /// <summary>
    /// Gets the type name of the request.
    /// </summary>
    [Persist]
    public string? RequestTypeName { get; private set; }

    /// <summary>
    /// Gets the unique key associated with the request.
    /// </summary>
    [Persist]
    public string? RequestKey { get; private set; }
}
