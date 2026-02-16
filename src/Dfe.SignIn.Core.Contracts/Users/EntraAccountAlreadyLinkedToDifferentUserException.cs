using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when a requested Entra OID is already linked to a different DSI user account.
/// </summary>
public sealed class EntraAccountAlreadyLinkedToDifferentUserException : InteractionException
{
    /// <summary>
    /// Creates an instance of the <see cref="EntraAccountAlreadyLinkedToDifferentUserException"/>.
    /// </summary>
    /// <param name="userId"> The ID of the DSI user that the Entra OID was intended to be linked to. </param>
    /// <param name="entraOid">The Entra Object ID that is already linked to another user. </param>
    /// <param name="existingUserId"> The ID of the DSI user account that the Entra OID is currently linked to.</param>
    public static EntraAccountAlreadyLinkedToDifferentUserException FromUserIds(
        Guid userId, Guid entraOid, Guid existingUserId) => new() {
            UserId = userId,
            EntraOid = entraOid,
            ExistingUserId = existingUserId,
        };

    /// <inheritdoc/>
    public EntraAccountAlreadyLinkedToDifferentUserException() { }

    /// <inheritdoc/>
    public EntraAccountAlreadyLinkedToDifferentUserException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public EntraAccountAlreadyLinkedToDifferentUserException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets the DSI user ID that the Entra OID is intended to link to.
    /// </summary>
    [Persist]
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the Entra OID that needs to be linked to a DSI user.
    /// </summary>
    [Persist]
    public Guid EntraOid { get; private set; }

    /// <summary>
    /// Gets the ID of the existing user that the Entra OID is currently linked to.
    /// </summary>
    [Persist]
    public Guid ExistingUserId { get; private set; }
}

