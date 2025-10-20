using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when a problem occurred whilst attempting to update an
/// authentication method for a user.
/// </summary>
public sealed class FailedToUpdateAuthenticationMethodException : InteractionException
{
    /// <inheritdoc/>
    public FailedToUpdateAuthenticationMethodException() { }

    /// <inheritdoc/>
    public FailedToUpdateAuthenticationMethodException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public FailedToUpdateAuthenticationMethodException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailedToUpdateAuthenticationMethodException"/> class.
    /// </summary>
    /// <param name="userId">The unique ID of the user.</param>
    public FailedToUpdateAuthenticationMethodException(Guid userId)
    {
        this.UserId = userId;
    }

    /// <summary>
    /// Gets the unique ID of the user.
    /// </summary>
    [Persist]
    public Guid? UserId { get; init; }
}
