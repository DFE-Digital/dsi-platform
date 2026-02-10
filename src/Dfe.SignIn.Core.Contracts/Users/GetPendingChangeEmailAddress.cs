using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request for a user to change their email address.
/// </summary>
[AssociatedResponse(typeof(GetPendingChangeEmailAddressResponse))]
[Throws(typeof(UserNotFoundException))]
[Throws(typeof(NoPendingChangeEmailException))]
public sealed record GetPendingChangeEmailAddressRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public Guid UserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="GetPendingChangeEmailAddressRequest"/>.
/// </summary>
public sealed record GetPendingChangeEmailAddressResponse
{
    /// <summary>
    /// The pending new email address if any; otherwise, a value of null.
    /// </summary>
    public PendingChangeEmailAddress? PendingChangeEmailAddress { get; init; }
}

/// <summary>
/// Represents a pending request for a user to change their email address.
/// </summary>
public sealed record PendingChangeEmailAddress
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The new email address.
    /// </summary>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string NewEmailAddress { get; init; }

    /// <summary>
    /// The verification code which can be used to verify the new email address.
    /// </summary>
    public required string VerificationCode { get; init; }

    /// <summary>
    /// The expiry time of the verification code.
    /// </summary>
    /// <seealso cref="HasExpired"/>
    public required DateTime ExpiryTimeUtc { get; init; }

    /// <summary>
    /// Indicates if the verification code has expired.
    /// </summary>
    /// <seealso cref="ExpiryTimeUtc"/>
    public bool HasExpired { get; init; }
}

/// <summary>
/// The exception thrown when there is no pending 'change email' request.
/// </summary>
public sealed class NoPendingChangeEmailException : InteractionException
{
    /// <inheritdoc/>
    public NoPendingChangeEmailException() { }

    /// <inheritdoc/>
    public NoPendingChangeEmailException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public NoPendingChangeEmailException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
