namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Contains the API routes for user-related endpoints.
/// </summary>
public static class UsersApiRoutes
{
    /// <summary>
    /// The API route for changing a user's name.
    /// </summary>
    public const string ChangeName = "/interaction/Users.ChangeName";

    /// <summary>
    /// The API route for determining if a user is an approver.
    /// </summary>
    public const string IsApprover = "/interaction/users/{userId}/is-approver";

    /// <summary>
    /// The API route for getting the number of pending approval requests
    /// </summary>
    public const string PendingApprovalCounter = "/interaction/users/{userId}/pending-approval-counter";
    
    /// The API route for retrieving a user's profile.
    /// </summary>
    public const string GetUserProfile = "/interaction/{userId}/Users.GetUserProfile";
}
