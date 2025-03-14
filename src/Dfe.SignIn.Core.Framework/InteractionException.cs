namespace Dfe.SignIn.Core.Framework;

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
    /// <inheritdoc/>
    protected InteractionException() { }

    /// <inheritdoc/>
    protected InteractionException(string? message) : base(message) { }

    /// <inheritdoc/>
    protected InteractionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
