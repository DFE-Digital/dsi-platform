using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to get the profile of a user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetUserProfileResponse"/></item>
///   </list>
///   <para>Throws <see cref="UserNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the user account was not found.</item>
///   </list>
/// </remarks>
public sealed record GetUserProfileRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="GetUserProfileRequest"/>.
/// </summary>
public sealed record GetUserProfileResponse
{
    /// <summary>
    /// A value indicating if the user is in Entra.
    /// </summary>
    public required bool IsEntra { get; init; }

    /// <summary>
    /// A value indicating if the user is an internal team member.
    /// </summary>
    public required bool IsInternalUser { get; init; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    [RegularExpression(StringPatterns.FirstNamePattern)]
    public required string FirstName { get; init; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    [RegularExpression(StringPatterns.LastNamePattern)]
    public required string LastName { get; init; }

    /// <summary>
    /// The job title of the user when set; otherwise, null.
    /// </summary>
    [RegularExpression(StringPatterns.JobTitlePattern)]
    public string? JobTitle { get; init; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }
}
