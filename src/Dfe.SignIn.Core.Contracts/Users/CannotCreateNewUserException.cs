using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when creating a new user account, but either the email address is
/// already in use, or the entra user id.
/// </summary>
public sealed class CannotCreateNewUserException : InteractionException
{
    /// <summary>
    /// Creates an instance of the <see cref="CannotCreateNewUserException"/> from an email address.
    /// </summary>
    /// <param name="emailAddress">The email address that is already in use.</param>
    public static CannotCreateNewUserException FromEmailAddress(string emailAddress)
        => new() { EmailAddress = emailAddress };

    /// <summary>
    /// Creates an instance of the <see cref="CannotCreateNewUserException"/> from an Entra user Id.
    /// </summary>
    /// <param name="entraUserId">The entra user id that is already in use.</param>
    public static CannotCreateNewUserException FromEntraUserId(Guid entraUserId)
        => new() { EntraUserId = entraUserId };

    /// <inheritdoc/>
    public CannotCreateNewUserException() { }

    /// <inheritdoc/>
    public CannotCreateNewUserException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public CannotCreateNewUserException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the email address of the account tha already exists.
    /// </summary>
    [Persist]
    public string? EmailAddress { get; private set; }

    /// <summary>
    /// Gets or sets the entra user id of the account tha already exists.
    /// </summary>
    [Persist]
    public Guid? EntraUserId { get; private set; }
}
