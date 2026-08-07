namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;

/// <summary>
/// Represents a request to change the job title for a user.
/// </summary>
public sealed record ChangeJobTitleRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's job title.
    /// </summary>
    public required string? NewJobTitle { get; init; }
}
