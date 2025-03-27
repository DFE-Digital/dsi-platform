using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// The exception thrown when a required claim type is missing.
/// </summary>
[Serializable]
public sealed class MissingClaimException : Exception
{
    /// <inheritdoc/>
    public MissingClaimException() { }

    /// <inheritdoc/>
    public MissingClaimException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public MissingClaimException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <inheritdoc cref="MissingClaimException(string)"/>
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
