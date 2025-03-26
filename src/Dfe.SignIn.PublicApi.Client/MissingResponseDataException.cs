namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// The exception that occurs when response data is missing from a public API request.
/// </summary>
[Serializable]
public sealed class MissingResponseDataException : Exception
{
    /// <inheritdoc/>
    public MissingResponseDataException() { }

    /// <inheritdoc/>
    public MissingResponseDataException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public MissingResponseDataException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
