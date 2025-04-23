namespace Dfe.SignIn.PublicApi.Client.PublicApiSigning;

/// <summary>
/// The exception thrown when no public keys were retrieved when attempting to refresh
/// public keys from the DfE Sign-in public API.
/// </summary>
[Serializable]
public sealed class NoPublicKeysWereFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoPublicKeysWereFoundException"/> class.
    /// </summary>
    public NoPublicKeysWereFoundException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoPublicKeysWereFoundException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public NoPublicKeysWereFoundException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoPublicKeysWereFoundException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public NoPublicKeysWereFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
