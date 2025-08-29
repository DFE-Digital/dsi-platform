namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// The exception that occurs when response data is missing from a public API request.
/// </summary>
public sealed class MissingResponseDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingResponseDataException"/> class.
    /// </summary>
    public MissingResponseDataException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingResponseDataException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingResponseDataException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingResponseDataException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingResponseDataException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
