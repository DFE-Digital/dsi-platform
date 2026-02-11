namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// The exception that occurs when something goes wrong whilst processing an
/// interaction (eg. use case, api request, etc).
/// </summary>
/// <remarks>
///   <para>Custom exception types should be defined for business logic errors which
///   can be thrown by use cases and propagated by intertim interactors. Interactions
///   exceptions must extend <see cref="InteractionException"/>.</para>
/// </remarks>
public abstract class InteractionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionException"/> class.
    /// </summary>
    protected InteractionException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionException"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected InteractionException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionException"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected InteractionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

/// <summary>
/// The exception that occurs when an item was not found whilst processing an
/// interaction (eg. use case, api request, etc).
/// </summary>
public abstract class NotFoundInteractionException : InteractionException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundInteractionException"/> class.
    /// </summary>
    protected NotFoundInteractionException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundInteractionException"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected NotFoundInteractionException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundInteractionException"/> class.
    /// </summary>
    /// <inheritdoc/>
    protected NotFoundInteractionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
