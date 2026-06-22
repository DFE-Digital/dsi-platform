using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;
/// <summary>
/// Session Configuration object with property validation
/// </summary>
public class SessionConfiguration
{
    /// <summary>
    /// Duration of active session
    /// </summary>
    [Required]
    public int? DurationInMinutes { get; set; }

    /// <summary>
    /// Interval when to notify user of remaning session window before timeout
    /// </summary>
    [Required]
    public int? NotifyRemainingMinutes { get; set; }
}
