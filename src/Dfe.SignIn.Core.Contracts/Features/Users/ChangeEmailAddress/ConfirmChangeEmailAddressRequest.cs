namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Represents a request to confirm that the user has verified their new email address.
/// </summary>
public sealed record ConfirmChangeEmailAddressRequest
{
    /// <summary>
    /// The user's email verification code.
    /// </summary>
    public required string VerificationCode { get; init; }
}
