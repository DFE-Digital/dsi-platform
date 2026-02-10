using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to change the email address of a user.
/// </summary>
[AssociatedResponse(typeof(InitiateChangeEmailAddressResponse))]
[Throws(typeof(UserNotFoundException))]
public sealed record InitiateChangeEmailAddressRequest : IKeyedRequest
{
    /// <summary>
    /// The client ID of the application that initiated the request.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's new email address.
    /// </summary>
    [Required(ErrorMessage = "Enter an email address")]
    [RegularExpression(StringPatterns.EmailAddressPattern, ErrorMessage = "Enter a valid email address")]
    public required string NewEmailAddress { get; init; }

    /// <summary>
    /// A value indicating if the request is being self-invoked by the user.
    /// </summary>
    public required bool IsSelfInvoked { get; init; }

    /// <inheritdoc/>
    string IKeyedRequest.Key => this.UserId.ToString();
}

/// <summary>
/// Represents a response for <see cref="InitiateChangeEmailAddressRequest"/>.
/// </summary>
public sealed record InitiateChangeEmailAddressResponse
{
}
