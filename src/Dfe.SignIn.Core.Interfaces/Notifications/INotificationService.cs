namespace Dfe.SignIn.Core.Interfaces.Notifications;

/// <summary>
/// Defines a service for sending email notifications using GOV.UK Notify.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification using GOV.UK Notify.
    /// </summary>
    /// <param name="recipientEmailAddress">The email address of the recipient.</param>
    /// <param name="templateId">The ID of the email template to use.</param>
    /// <param name="personalisation">A dictionary of personalisation values for the email template.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendAsync(string recipientEmailAddress, string templateId, IReadOnlyDictionary<string, dynamic> personalisation);
}
