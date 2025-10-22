namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request for a user to change their email address.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetPendingChangeEmailAddressResponse"/></item>
///   </list>
///   <para>Throws <see cref="UserNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the user account was not found.</item>
///   </list>
/// </remarks>
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
    /// The new email address.
    /// </summary>
    public required string NewEmailAddress { get; init; }

    /// <summary>
    /// The verification code which can be used to verify the new email address.
    /// </summary>
    public required string VerificationCode { get; init; }
}
