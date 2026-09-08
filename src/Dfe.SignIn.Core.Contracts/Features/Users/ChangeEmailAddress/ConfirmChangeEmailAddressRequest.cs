using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Core.Contracts.Common;

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

/// <summary>
/// Represents the response to a request to confirm that the user has verified their new email address.
/// </summary>
public sealed record ConfirmChangeEmailAddressResponse : ApiResponseWithWarnings
{
    /// <summary>
    /// The new email address that the user has verified.
    /// </summary>
    public required string NewEmailAddress { get; set; }
}

/// <summary>
/// Contains warning codes related to the change email address process.
/// </summary>
public static class ChangeEmailWarnings
{
    /// <summary>
    /// Indicates that the Entra MFA sync failed during the change email address process.
    /// </summary>
    public const string EntraMfaSyncFailed = "ChangeEmail.EntraMfaSyncFailed";

    /// <summary>
    /// Creates a new warning indicating that the Entra MFA sync failed during the change email address process.
    /// </summary>
    /// <param name="detail">The detail of the warning.</param>
    /// <returns>The created warning.</returns>
    public static Warning EntraMfaSyncFailedWarning(string detail)
    {
        return new(EntraMfaSyncFailed, detail);
    }
}

/// <summary>
/// Contains error codes and factory methods related to the change email address process.
/// </summary>
public static class ChangeEmailErrors
{
    /// <summary>
    /// Defines error codes for change email operations.
    /// </summary>
    public static class Codes
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
    }

    /// <summary>
    /// Indicates that no pending change email request was found for the user.
    /// </summary>
    public const string NoPendingRequest = Codes.NoPendingRequest;

    /// <summary>
    /// Indicates that the verification code provided is incorrect.
    /// </summary>
    public const string InvalidCode = Codes.InvalidCode;

    /// <summary>
    /// Indicates that the verification code has expired.
    /// </summary>
    public const string CodeExpired = Codes.CodeExpired;

    /// <summary>
    /// Indicates that the pending change email request is invalid.
    /// </summary>
    public const string InvalidRequest = Codes.InvalidRequest;

    /// <summary>
    /// Creates an error indicating that no pending change email request was found.
    /// </summary>
    public static Error NoPendingRequestError()
        => new(Codes.NoPendingRequest, "No pending change email request found");

    /// <summary>
    /// Creates an error indicating that the verification code entered is incorrect.
    /// </summary>
    public static Error InvalidCodeError()
        => new(Codes.InvalidCode, "The verification code you entered is incorrect");

    /// <summary>
    /// Creates an error indicating that the verification code has expired.
    /// </summary>
    public static Error CodeExpiredError()
        => new(Codes.CodeExpired, "The verification code has expired");

    /// <summary>
    /// Creates an error indicating that the pending change email request is invalid.
    /// </summary>
    public static Error InvalidRequestError()
        => new(Codes.InvalidRequest, "The pending change email request is invalid.");
}

