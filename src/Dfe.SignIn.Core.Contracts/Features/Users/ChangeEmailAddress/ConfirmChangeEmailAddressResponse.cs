using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Core.Contracts.Common;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

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
