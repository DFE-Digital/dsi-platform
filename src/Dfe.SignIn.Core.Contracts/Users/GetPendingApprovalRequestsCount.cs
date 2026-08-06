namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a response
/// </summary>
public sealed record PendingApprovalCountResponse
{
    /// <summary>
    /// The number of pending requests
    /// </summary>
    public int Count { get; set; }
}
