using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Contains error codes and factory methods related to the change email address process.
/// </summary>
public static class ChangeEmailErrors
{
    /// <summary>
    /// Indicates that no pending change email request was found for the user.
    /// </summary>
    public const string NoPendingRequest = "ChangeEmail.NoPendingRequest";

    /// <summary>
    /// Indicates that the verification code provided is incorrect.
    /// </summary>
    public const string InvalidCode = "ChangeEmail.InvalidCode";

    /// <summary>
    /// Indicates that the verification code has expired.
    /// </summary>
    public const string CodeExpired = "ChangeEmail.CodeExpired";

    /// <summary>
    /// Indicates that the pending change email request is invalid.
    /// </summary>
    public const string InvalidRequest = "ChangeEmail.InvalidRequest";

    /// <summary>
    /// Creates an error indicating that no pending change email request was found.
    /// </summary>
    public static Error NoPendingRequestError()
        => new(NoPendingRequest, "No pending change email request found");

    /// <summary>
    /// Creates an error indicating that the verification code entered is incorrect.
    /// </summary>
    public static Error InvalidCodeError()
        => new(InvalidCode, "The verification code you entered is incorrect");

    /// <summary>
    /// Creates an error indicating that the verification code has expired.
    /// </summary>
    public static Error CodeExpiredError()
        => new(CodeExpired, "The verification code has expired");

    /// <summary>
    /// Creates an error indicating that the pending change email request is invalid.
    /// </summary>
    public static Error InvalidRequestError()
        => new(InvalidRequest, "The pending change email request is invalid.");
}

