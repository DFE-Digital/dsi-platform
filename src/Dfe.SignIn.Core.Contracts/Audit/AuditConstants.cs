namespace Dfe.SignIn.Core.Contracts.Audit;

/// <summary>
/// Defines the names of the available audit event types.
/// </summary>
public static class AuditEventCategoryNames
{
    /// <summary>
    /// An event that is raised by the support team.
    /// </summary>
    /// <seealso cref="AuditSupportEventNames"/>
    public const string Support = "support";

    /// <summary>
    /// An event that is raised when a user is changing their email address.
    /// </summary>
    public const string ChangeEmail = "change-email";

    /// <summary>
    /// An event that is raised when a user is changing their job title.
    /// </summary>
    public const string ChangeJobTitle = "change-job-title";

    /// <summary>
    /// An event that is raised when a user is changing their first and/or last name.
    /// </summary>
    public const string ChangeName = "change-name";

    /// <summary>
    /// An event that is raised when a user is changing their password.
    /// </summary>
    /// <seealso cref="AuditChangePasswordEventNames"/>
    public const string ChangePassword = "change-password";
}

/// <summary>
/// Event names for the <see cref="AuditEventCategoryNames.Support"/> audit event category.
/// </summary>
public static class AuditSupportEventNames
{
    /// <summary>
    /// Indicates that the "user search" feature was used.
    /// </summary>
    public const string UserSearch = "user-search";
}

/// <summary>
/// Event names for the <see cref="AuditEventCategoryNames.ChangePassword"/> audit event category.
/// </summary>
public static class AuditChangePasswordEventNames
{
    /// <summary>
    /// Indicates that an incorrect password was provided.
    /// </summary>
    public const string IncorrectPassword = "incorrect-password";
}
