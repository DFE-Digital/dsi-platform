using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Contains error codes and factory methods related to the change email address process.
/// </summary>
public static class ChangeEmailErrors
{
    /// <summary>
    /// Creates an error indicating that no pending change email request was found.
    /// </summary>
    public static readonly Error NoPendingRequest
        = new("ChangeEmail.NoPendingRequest", "No pending change email request found");

    /// <summary>
    /// Creates an error indicating that the verification code entered is incorrect.
    /// </summary>
    public static readonly Error InvalidCode
        = new("ChangeEmail.InvalidCode", "The verification code you entered is incorrect");

    /// <summary>
    /// Creates an error indicating that the verification code has expired.
    /// </summary>
    public static readonly Error CodeExpired
        = new("ChangeEmail.CodeExpired", "The verification code has expired");

    /// <summary>
    /// Creates an error indicating that the pending change email request is invalid.
    /// </summary>
    public static readonly Error InvalidRequest
        = new("ChangeEmail.InvalidRequest", "The pending change email request is invalid.");
}
