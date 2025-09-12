using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when an inactive user cannot be linked.
/// </summary>
public sealed class CannotLinkInactiveUserException : InteractionException
{
    /// <inheritdoc/>
    public CannotLinkInactiveUserException() { }

    /// <inheritdoc/>
    public CannotLinkInactiveUserException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public CannotLinkInactiveUserException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
