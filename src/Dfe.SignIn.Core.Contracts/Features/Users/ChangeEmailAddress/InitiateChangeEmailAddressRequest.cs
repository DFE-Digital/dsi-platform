using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Represents a request to change the email address of a user (alternative version).
/// </summary>
public sealed record InitiateChangeEmailAddressRequest : IKeyedRequest
{
    /// <inheritdoc/>
    public InitiateChangeEmailAddressRequest(string clientId, string newEmailAddress, bool isSelfInvoked)
    {
        this.ClientId = clientId;
        this.NewEmailAddress = newEmailAddress;
        this.IsSelfInvoked = isSelfInvoked;
    }

    /// <summary>
    /// Gets the client ID associated with the request.
    /// </summary>
    public string ClientId { get; init; }

    /// <summary>
    /// Gets the new email address to be set for the user.
    /// </summary>
    public string NewEmailAddress { get; init; }

    /// <summary>
    /// Gets a value indicating whether the request is self-invoked (initiated by the user themselves).
    /// </summary>
    public bool IsSelfInvoked { get; init; }

    /// <summary>
    /// Gets the unique key associated with the request, which is the string representation of the user ID.
    /// </summary>
    public string Key => string.Empty;
}
