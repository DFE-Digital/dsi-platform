using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to link an Entra user to DfE Sign-in.
/// </summary>
[AssociatedResponse(typeof(LinkEntraUserToDsiResponse))]
public sealed record LinkEntraUserToDsiRequest
{
    /// <summary>
    /// The unique ID of the user in DfE Sign-in.
    /// </summary>
    public required Guid DsiUserId { get; init; }

    /// <summary>
    /// The unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    [MinLength(1)]
    public required string FirstName { get; init; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    [MinLength(1)]
    public required string LastName { get; init; }
}

/// <summary>
/// Represents a response for <see cref="LinkEntraUserToDsiRequest"/>.
/// </summary>
public sealed record LinkEntraUserToDsiResponse
{
}
