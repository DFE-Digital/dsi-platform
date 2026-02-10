using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to cancel a previous user request to change their email address.
/// </summary>
[AssociatedResponse(typeof(CancelPendingChangeEmailAddressResponse))]
[Throws(typeof(UserNotFoundException))]
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
