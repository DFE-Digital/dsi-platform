namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// Application Configuration for Notifications
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Configuration key name
    /// </summary>
    public static string SectionName = "Notifications";

    /// <summary>
    /// Support team mailbox address
    /// </summary>
    public string SupportTeamEmail { get; init; } = string.Empty;
}
