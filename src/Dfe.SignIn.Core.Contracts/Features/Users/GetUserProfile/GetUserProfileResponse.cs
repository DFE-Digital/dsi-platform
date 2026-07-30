namespace Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;

/// <summary>
/// 
/// </summary>
public sealed record GetUserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string EmailAddress,
    bool IsEntra,
    bool IsInternalUser,
    string? JobTitle,
    short Status
);
//{
//    /// <summary>
//    /// A value indicating if the user is in Entra.
//    /// </summary>
//    public required bool IsEntra { get; init; }

//    /// <summary>
//    /// A value indicating if the user is an internal team member.
//    /// </summary>
//    public required bool IsInternalUser { get; init; }

//    /// <summary>
//    /// The first name of the user.
//    /// </summary>
//    public required string FirstName { get; init; }

//    /// <summary>
//    /// The last name of the user.
//    /// </summary>
//    public required string LastName { get; init; }

//    /// <summary>
//    /// The job title of the user when set; otherwise, null.
//    /// </summary>
//    public string? JobTitle { get; init; }

//    /// <summary>
//    /// The email address of the user.
//    /// </summary>
//    public required string EmailAddress { get; init; }

//    /// <summary>
//    /// The user status.
//    /// </summary>
//    public short Status { get; init; }
//}
