namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// The exception that occurs when the required type of interactor is missing.
/// </summary>
public class MissingInteractorException : InteractionException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingInteractorException"/> class.
    /// </summary>
    public MissingInteractorException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingInteractorException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingInteractorException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingInteractorException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingInteractorException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingInteractorException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingInteractorException(string? message, string requestType)
        : base(message)
    {
        this.RequestType = requestType;
    }

    /// <summary>
    /// Gets the name of the request model type.
    /// </summary>
    [Persist]
    public string RequestType { get; init; } = "Unspecified";
}
