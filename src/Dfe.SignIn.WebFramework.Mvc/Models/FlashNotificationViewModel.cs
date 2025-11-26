using GovUk.Frontend.AspNetCore;

namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// The view model for a flash notification.
/// </summary>
/// <remarks>
///   <para>A flash notification can be presented using extension methods from
///   the <see cref="FlashNotificationExtensions"/> class.</para>
/// </remarks>
public sealed record FlashNotificationViewModel
{
    /// <summary>
    /// The type of flash notification; for example, information or success.
    /// </summary>
    public NotificationBannerType Type { get; init; } = NotificationBannerType.Default;

    /// <summary>
    /// Optional, heading text for the flash notification.
    /// </summary>
    public string? Heading { get; init; }

    /// <summary>
    /// Message text for the flash notification.
    /// </summary>
    public required string Message { get; init; }
}
