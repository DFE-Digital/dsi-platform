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
