using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to create a new user in DfE Sign-in.
/// </summary>
[AssociatedResponse(typeof(CreateUserResponse))]
public sealed record CreateUserRequest
{
    /// <summary>
    /// The unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }

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
/// Represents a response for <see cref="CreateUserRequest"/>.
/// </summary>
public sealed record CreateUserResponse
{
    /// <summary>
    /// The unique ID of the user in DfE Sign-in.
    /// </summary>
    public required Guid UserId { get; init; }
}
