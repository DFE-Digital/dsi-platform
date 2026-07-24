namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;

/// <summary>
/// Represents a request to change the name of a user.
/// </summary>
public sealed record ChangeNameRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; init; }
}

/// <summary>
/// 
/// </summary>
public sealed record ChangeNameResponse
{
}
