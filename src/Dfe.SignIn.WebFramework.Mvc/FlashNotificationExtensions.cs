using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Mvc.Models;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Provides flash notification extensions for <see cref="Controller"/> implementations.
/// </summary>
public static class FlashNotificationExtensions
{
    /// <summary>
    /// The unique 'TempData' key for flash notifications.
    /// </summary>
    public const string TempDataKey = "FlashNotification";

    /// <summary>
    /// Sets flash notification indicating with information.
    /// </summary>
    /// <param name="controller">The current controller.</param>
    /// <param name="message">A message with the relevant information.</param>
    /// <param name="heading">An optional heading for the flash notification.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="message"/> is null or empty.</para>
    /// </exception>
    public static void SetFlashNotification(this Controller controller, string message, string? heading = null)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(message, nameof(message));

        controller.TempData[TempDataKey] = JsonSerializer.Serialize(new FlashNotificationViewModel {
            Heading = heading,
            Message = message,
        });
    }

    /// <summary>
    /// Sets flash notification indicating a successful transaction has occurred.
    /// </summary>
    /// <param name="controller">The current controller.</param>
    /// <param name="message">A message with information about the successful transaction.</param>
    /// <param name="heading">An optional heading for the flash notification.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="message"/> is null or empty.</para>
    /// </exception>
    public static void SetFlashSuccess(this Controller controller, string message, string? heading = null)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(message, nameof(message));

        controller.TempData[TempDataKey] = JsonSerializer.Serialize(new FlashNotificationViewModel {
            Type = NotificationBannerType.Success,
            Heading = heading,
            Message = message,
        });
    }

    /// <summary>
    /// Gets the current flash notification if one is present.
    /// </summary>
    /// <param name="tempData">The temp data dictionary to check.</param>
    /// <returns>
    ///   <para>A view model representing the flash notification when a flash
    ///   notification is present; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="tempData"/> is null.</para>
    /// </exception>
    public static FlashNotificationViewModel? GetFlashNotification(this ITempDataDictionary tempData)
    {
        ExceptionHelpers.ThrowIfArgumentNull(tempData, nameof(tempData));

        return (tempData[TempDataKey] is string notificationJson)
            ? JsonSerializer.Deserialize<FlashNotificationViewModel>(notificationJson)
            : null;
    }
}
