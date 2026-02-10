using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to confirm that the user has verified their new email address.
/// </summary>
[AssociatedResponse(typeof(ConfirmChangeEmailAddressResponse))]
[Throws(typeof(UserNotFoundException))]
public sealed record ConfirmChangeEmailAddressRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's email verification code.
    /// </summary>
    [Required(ErrorMessage = "Please enter verification code")]
    public required string VerificationCode { get; init; }
}

/// <summary>
/// Represents a response for <see cref="ConfirmChangeEmailAddressRequest"/>.
/// </summary>
public sealed record ConfirmChangeEmailAddressResponse
{
}
