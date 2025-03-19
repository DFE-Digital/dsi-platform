namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// The exception thrown when an unexpected exception occurs whilst processing an
/// interaction; for example, an underlying system error.
/// </summary>
public sealed class UnexpectedException : InteractionException
{
    /// <inheritdoc/>
    public UnexpectedException() { }

    /// <inheritdoc/>
    public UnexpectedException(string? message) : base(message) { }

    /// <inheritdoc/>
    public UnexpectedException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
