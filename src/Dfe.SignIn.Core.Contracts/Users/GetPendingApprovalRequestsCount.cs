using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Features.Users.Exceptions;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to retrieve the number of Pending Approvals
/// </summary>
[AssociatedResponse(typeof(PendingApprovalCountResponse))]
[Throws(typeof(UserNotFoundException))]
public sealed record GetPendingApprovalCountRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }
}

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
