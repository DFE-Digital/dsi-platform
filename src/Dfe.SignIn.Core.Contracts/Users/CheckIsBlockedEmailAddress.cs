using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to check whether an email address would be blocked.
/// </summary>
/// <seealso cref="CheckIsBlockedEmailAddressResponse"/>
public sealed record CheckIsBlockedEmailAddressRequest
{
    /// <summary>
    /// Gets the email address that is to be checked.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [EmailAddress]
    public required string EmailAddress { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CheckIsBlockedEmailAddressRequest"/>.
/// </summary>
public sealed record CheckIsBlockedEmailAddressResponse
{
    /// <summary>
    /// Gets a value indicating whether the email address has been blocked.
    /// </summary>
    public bool IsBlocked { get; init; }
}
