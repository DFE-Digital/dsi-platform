namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Response payload containing details of a user's pending email change request.
/// </summary>
public sealed record GetPendingChangeEmailResponse
{
    /// <summary>
    /// The new email address requested.
    /// </summary>
    public required string NewEmailAddress { get; init; }

    /// <summary>
    /// The UTC timestamp when the verification code was created.
    /// </summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// The UTC timestamp when the verification code expires (1 hour after creation).
    /// </summary>
    public required DateTime ExpiryTimeUtc { get; init; }

    /// <summary>
    /// Indicates whether the verification code has expired.
    /// </summary>
    public required bool HasExpired { get; init; }
}
