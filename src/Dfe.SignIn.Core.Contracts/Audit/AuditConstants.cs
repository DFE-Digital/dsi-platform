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
