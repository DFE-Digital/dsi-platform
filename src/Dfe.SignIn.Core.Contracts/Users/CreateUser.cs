using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to create a new user in DfE Sign-in.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="CreateUserResponse"/></item>
///   </list>
/// </remarks>
public sealed record CreateUserRequest
{
    /// <summary>
    /// Gets the unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    [MinLength(1)]
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the last name of the user.
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
    /// Gets the unique ID of the user in DfE Sign-in.
    /// </summary>
    public required Guid UserId { get; init; }
}
