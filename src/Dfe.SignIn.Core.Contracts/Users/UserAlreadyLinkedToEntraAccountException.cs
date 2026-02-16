using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// The exception thrown when a DSI user account is already linked to an Entra OID,
/// but an attempt is made to link it to a different Entra OID.
/// </summary>
public sealed class UserAlreadyLinkedToEntraAccountException : InteractionException
{
    /// <summary>
    /// Creates an instance of the <see cref="UserAlreadyLinkedToEntraAccountException"/>.
    /// </summary>
    /// <param name="userId">The DSI user Id.</param>
    /// <param name="entraOid">The Entra Oid that the DSI user account is currently linked to.</param>
    /// <param name="targetEntraId">The Entra Oid that the DSI user account was intended to be linked to.</param>
    public static UserAlreadyLinkedToEntraAccountException FromUserIds(
        Guid userId, Guid entraOid, Guid targetEntraId) => new() {
            UserId = userId,
            EntraOid = entraOid,
            TargetEntraId = targetEntraId,
        };

    /// <inheritdoc/>
    public UserAlreadyLinkedToEntraAccountException() { }

    /// <inheritdoc/>
    public UserAlreadyLinkedToEntraAccountException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public UserAlreadyLinkedToEntraAccountException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// The DSI user Id.
    /// </summary>
    [Persist]
    public Guid UserId { get; private set; }

    /// <summary>
    /// The Entra Oid that the DSI user account is currently linked to.
    /// </summary>
    [Persist]
    public Guid EntraOid { get; private set; }

    /// <summary>
    /// The Entra Oid that the DSI user account was intended to be linked to.
    /// </summary>
    [Persist]
    public Guid TargetEntraId { get; private set; }
}

