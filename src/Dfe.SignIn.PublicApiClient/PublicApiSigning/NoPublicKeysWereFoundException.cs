namespace Dfe.SignIn.PublicApiClient.PublicApiSigning;

/// <summary>
/// The exception thrown when no public keys were retrieved when attempting to refresh
/// public keys from the DfE Sign-in public API.
/// </summary>
public sealed class NoPublicKeysWereFoundException : Exception
{
    /// <inheritdoc/>
    public NoPublicKeysWereFoundException() { }

    /// <inheritdoc/>
    public NoPublicKeysWereFoundException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public NoPublicKeysWereFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
