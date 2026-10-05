using Dfe.SignIn.WebFramework.Extensions;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Application Configuration for Notifications
/// </summary>
public class NotificationSettings : IApplicationSettings
{
    /// <summary>
    /// Configuration key name
    /// </summary>
    static string IApplicationSettings.SectionName => "Notifications";

    /// <summary>
    /// Support team mailbox address
    /// </summary>
    public string SupportTeamEmail { get; init; } = string.Empty;
}

/// <summary>
/// Defines a static class containing constants for notification template IDs used in the application.
/// </summary>
public static class NotificationTemplateIds
{
    /// <summary>
    /// The template ID for the "Verify Change Email" notification, used when a user initiates a change of their email address and needs to verify the new email.
    /// </summary>
    public static readonly string VerifyChangeEmail = "8a6b7625-87d5-41bc-bc58-035343571d81";

    /// <summary>
    /// The template ID for the "Notify Migrated Email" notification, used to inform users that their email address has been successfully migrated or changed.
    /// </summary>
    public static readonly string NotifyMigratedEmail = "18e0e804-04c6-4f73-9462-ab3cbf8b990f";
}
