using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Represents a request to change the email address of a user.
/// </summary>
public sealed record InitiateChangeEmailAddressRequest(
    string ClientId,
    Guid UserId,
    string NewEmailAddress,
    bool IsSelfInvoked) : IKeyedRequest
{
    /// <summary>
    /// Gets the key for the request, which is the user ID.
    /// This is used for request deduplication and tracking.
    /// </summary>
    public string Key => this.UserId.ToString();
}
//{
//    /// <summary>
//    /// The client ID of the application that initiated the request.
//    /// </summary>
//    public required string ClientId { get; init; }

//    /// <summary>
//    /// The unique ID of the user.
//    /// </summary>
//    public required Guid UserId { get; init; }

//    /// <summary>
//    /// The user's new email address.
//    /// </summary>
//    public required string NewEmailAddress { get; init; }

//    /// <summary>
//    /// A value indicating if the request is being self-invoked by the user.
//    /// </summary>
//    public required bool IsSelfInvoked { get; init; }
//}
