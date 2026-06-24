namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;

/// <summary>
/// A request to change the job title of a user.
/// </summary>
public sealed record ChangeJobTitleRequest
{
    /// <summary>
    /// The unique identifier of the user whose job title is to be changed.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The new job title to set for the user.
    /// </summary>
    public required string NewJobTitle { get; init; }
}
