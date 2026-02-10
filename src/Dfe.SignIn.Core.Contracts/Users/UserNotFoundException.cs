using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when a requested user account was not found.
/// </summary>
public sealed class UserNotFoundException : NotFoundInteractionException
{
    /// <summary>
    /// Creates an instance of the <see cref="UserNotFoundException"/> from a user ID.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    public static UserNotFoundException FromUserId(Guid userId)
        => new() { UserId = userId };

    /// <inheritdoc/>
    public UserNotFoundException() { }

    /// <inheritdoc/>
    public UserNotFoundException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public UserNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the ID of the user that was not found.
    /// </summary>
    [Persist]
    public Guid? UserId { get; private set; }
}
