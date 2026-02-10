using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to change the name of a user.
/// </summary>
[AssociatedResponse(typeof(ChangeNameResponse))]
[Throws(typeof(UserNotFoundException))]
public sealed record ChangeNameRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    [Required(ErrorMessage = "Enter a first name")]
    [RegularExpression(StringPatterns.FirstNamePattern, ErrorMessage = "Enter a valid first name")]
    [MaxLength(60, ErrorMessage = "Enter a name with no more than 60 characters")]
    public required string FirstName { get; init; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    [Required(ErrorMessage = "Enter a last name")]
    [RegularExpression(StringPatterns.LastNamePattern, ErrorMessage = "Enter a valid last name")]
    [MaxLength(60, ErrorMessage = "Enter a name with no more than 60 characters")]
    public required string LastName { get; init; }
}

/// <summary>
/// Represents a response for <see cref="ChangeNameRequest"/>.
/// </summary>
public sealed record ChangeNameResponse
{
}
