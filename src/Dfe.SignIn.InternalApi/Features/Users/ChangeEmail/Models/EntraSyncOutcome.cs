using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail.Models;

/// <summary>
/// The result of synchronising a confirmed email change with Microsoft Entra ID.
/// </summary>
public enum EntraSyncStatus
{
    /// <summary>
    /// The user is not linked to Entra; no external sync was attempted.
    /// </summary>
    NotApplicable,

    /// <summary>
    /// Entra sync completed successfully, or was not required.
    /// </summary>
    Succeeded,

    /// <summary>
    /// DSI was updated but Entra MFA email sync failed; manual intervention is required.
    /// </summary>
    MfaSyncFailed,

    /// <summary>
    /// Entra sync failed and the DSI email change was rolled back.
    /// </summary>
    HardFailure,
}

/// <summary>
/// Outcome of an Entra email synchronisation attempt during confirm change email.
/// </summary>
/// <param name="Status">The overall sync status.</param>
/// <param name="Error">The error detail when <see cref="Status"/> is <see cref="EntraSyncStatus.MfaSyncFailed"/> or <see cref="EntraSyncStatus.HardFailure"/>.</param>
public sealed record EntraSyncOutcome(EntraSyncStatus Status, Error? Error = null);
