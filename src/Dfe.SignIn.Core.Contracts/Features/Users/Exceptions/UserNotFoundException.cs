using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Features.Users.Exceptions;

/// <summary>
/// The exception thrown when a requested user account was not found.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "We could come back and test this, but it is not worth the effort for now.")]
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
