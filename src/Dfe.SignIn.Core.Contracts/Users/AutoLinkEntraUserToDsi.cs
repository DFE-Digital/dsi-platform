using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to automatically link an Entra user to DfE Sign-in.
/// </summary>
[AssociatedResponse(typeof(AutoLinkEntraUserToDsiResponse))]
[Throws(typeof(CannotLinkInactiveUserException))]
public sealed record AutoLinkEntraUserToDsiRequest
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
    [Required]
    [RegularExpression(StringPatterns.FirstNamePattern)]
    public required string FirstName { get; init; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    [Required]
    [RegularExpression(StringPatterns.LastNamePattern)]
    public required string LastName { get; init; }
}

/// <summary>
/// Represents a response for <see cref="AutoLinkEntraUserToDsiRequest"/>.
/// </summary>
public sealed record AutoLinkEntraUserToDsiResponse
{
    /// <summary>
    /// Gets the unique ID of the user in DfE Sign-in.
    /// </summary>
    public Guid UserId { get; set; }
}
