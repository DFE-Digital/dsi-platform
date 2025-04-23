using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// The exception thrown when a required claim type is missing.
/// </summary>
[Serializable]
public sealed class MissingClaimException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingClaimException"/> class.
    /// </summary>
    public MissingClaimException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingClaimException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingClaimException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingClaimException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public MissingClaimException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingClaimException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="claimType">The type of claim.</param>
    public MissingClaimException(string? message, string claimType)
        : base(message)
    {
        this.ClaimType = claimType;
    }

    /// <summary>
    /// Gets the type of claim that was required.
    /// </summary>
    [Persist]
    public string? ClaimType { get; } = null;
}
