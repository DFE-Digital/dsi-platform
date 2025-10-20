using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to change the job title of a user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="ChangeJobTitleResponse"/></item>
///   </list>
///   <para>Throws <see cref="UserNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the user account was not found.</item>
///   </list>
/// </remarks>
public sealed record ChangeJobTitleRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's new job title.
    /// </summary>
    [RegularExpression(@"^[\p{L}\p{N} ()]*$", ErrorMessage = "Special characters cannot be used")]
    [MaxLength(60, ErrorMessage = "Enter a job title with no more than 60 characters")]
    public required string NewJobTitle { get; init; }
}

/// <summary>
/// Represents a response for <see cref="ChangeJobTitleRequest"/>.
/// </summary>
public sealed record ChangeJobTitleResponse
{
}
