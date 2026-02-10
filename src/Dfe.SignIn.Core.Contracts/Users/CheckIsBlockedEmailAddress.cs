using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to check whether an email address would be blocked.
/// </summary>
[AssociatedResponse(typeof(CheckIsBlockedEmailAddressResponse))]
public sealed record CheckIsBlockedEmailAddressRequest
{
    /// <summary>
    /// The email address that is to be checked.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CheckIsBlockedEmailAddressRequest"/>.
/// </summary>
public sealed record CheckIsBlockedEmailAddressResponse
{
    /// <summary>
    /// A value indicating whether the email address has been blocked.
    /// </summary>
    public bool IsBlocked { get; init; }
}
