namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to cancel a previous user request to change their email address.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="CancelPendingChangeEmailAddressResponse"/></item>
///   </list>
///   <para>Throws <see cref="UserNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the user account was not found.</item>
///   </list>
/// </remarks>
public sealed record CancelPendingChangeEmailAddressRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public Guid UserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CancelPendingChangeEmailAddressRequest"/>.
/// </summary>
public sealed record CancelPendingChangeEmailAddressResponse
{
}
