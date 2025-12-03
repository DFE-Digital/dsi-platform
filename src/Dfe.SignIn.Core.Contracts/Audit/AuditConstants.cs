namespace Dfe.SignIn.Core.Contracts.Audit;

/// <summary>
/// Defines the names of the available audit event types.
/// </summary>
public static class AuditEventCategoryNames
{
    /// <summary>
    /// An event that is raised when a user signs out.
    /// </summary>
    public const string SignOut = "Sign-out";

    /// <summary>
    /// An event that is raised by the support team.
    /// </summary>
    /// <seealso cref="AuditSupportEventNames"/>
    public const string Support = "support";

    /// <summary>
    /// An event that is raised when a user is changing their email address.
    /// </summary>
    /// <seealso cref="AuditChangeEmailEventNames"/>
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
/// Event names for the <see cref="AuditEventCategoryNames.ChangeEmail"/> audit event category.
/// </summary>
public static class AuditChangeEmailEventNames
{
    /// <summary>
    /// Indicates that a request was made to change a user email address.
    /// </summary>
    public const string RequestToChangeEmail = "request-to-change-email";

    /// <summary>
    /// Indicates that the user attempted to use an existing email address.
    /// </summary>
    public const string RequestedExistingEmail = "requested-existing-email";

    /// <summary>
    /// Indicates that the change request was cancelled.
    /// </summary>
    public const string CancelChangeEmail = "cancel-change-email";

    /// <summary>
    /// Indicates that an expired email verification code was entered.
    /// </summary>
    public const string EnteredExpiredCode = "entered-expired-code";

    /// <summary>
    /// Indicates that an error occurred whilst attempting to change the email address.
    /// </summary>
    public const string EmailChangeFailed = "email-change-failed";
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
