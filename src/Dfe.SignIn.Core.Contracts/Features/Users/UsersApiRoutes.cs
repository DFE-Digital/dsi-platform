namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Contains the API routes for user-related endpoints.
/// </summary>
public static class UsersApiRoutes
{
    /// <summary>
    /// The API route for changing a user's name.
    /// </summary>
    public const string ChangeName = "/internal/Users.ChangeName";

    /// <summary>
    /// The API route for determining if a user is an approver.
    /// </summary>
    public const string IsApprover = "/internal/users/{userId}/is-approver";

    /// <summary>
    /// The API route for getting the number of pending approval requests
    /// </summary>
    public const string PendingApprovalCounter = "/internal/users/{userId}/pending-approval-counter";

    /// <summary>
    /// The API route for retrieving a user's profile.
    /// </summary>
    public const string GetUserProfile = "/internal/{userId}/Users.GetUserProfile";

    /// <summary>
    /// The API route for changing a user's job title.
    /// </summary>
    public const string ChangeJobTitle = "/internal/users/{userId}/job-title";

    /// <summary>
    /// The API route for changing a user's email address.
    /// </summary>
    public const string InitiateChangeEmail = "/internal/users/{userId}/initiate-change-email";

    /// <summary>
    /// The API route for confirming the change of a user's email address.
    /// </summary>
    public const string ConfirmChangeEmail = "/internal/users/{userId}/confirm-change-email";
}
