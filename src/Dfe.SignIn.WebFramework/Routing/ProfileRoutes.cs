namespace Dfe.SignIn.WebFramework.Routing;

/// <summary>
/// Defines a static class containing route constants for profile-related actions in the application.
/// </summary>
public static class ProfileRoutes
{
    /// <summary>
    /// The route for initiating the change of a user's email address.
    /// </summary>
    public const string ChangeEmailVerification = "/change-email/verify";

    /// <summary>
    /// Gets the route for verifying a change to a specific user's email address.
    /// </summary>
    public static string ChangeEmailVerificationForUser(Guid userId) => $"/change-email/{userId}/verify";
}
