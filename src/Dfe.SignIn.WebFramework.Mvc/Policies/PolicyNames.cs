namespace Dfe.SignIn.WebFramework.Mvc.Policies;

/// <summary>
/// Contains constant values representing authorization policies used in an
/// ASP.NET Core MVC application. These policies define specific permissions
/// that can be applied throughout the application.
/// </summary>
public static class PolicyNames
{
    #region Own User Account Management

    /// <summary>
    /// Authorization policy that allows a user to change their own email address.
    /// </summary>
    public const string CanChangeOwnEmailAddress = nameof(CanChangeOwnEmailAddress);

    /// <summary>
    /// Authorization policy that allows a user to change their own password.
    /// </summary>
    public const string CanChangeOwnPassword = nameof(CanChangeOwnPassword);

    #endregion
}
