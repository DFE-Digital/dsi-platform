using Microsoft.Extensions.Logging;
using Notify.Exceptions;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify;

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
    Task SendAsync(string recipientEmailAddress, string templateId, Dictionary<string, dynamic> personalisation);
}

/// <summary>
/// An implementation of <see cref="INotificationService"/> that uses GOV.UK Notify to send email notifications.
/// </summary>
/// <param name="notificationClient">The GOV.UK Notify client.</param>
/// <param name="logger">The logger for logging information and errors.</param>
public class EmailNotificationServiceWithGovNotify(
    IAsyncNotificationClient notificationClient,
    ILogger<EmailNotificationServiceWithGovNotify> logger)
    : INotificationService
{
    /// <inheritdoc/>
    public async Task SendAsync(string recipientEmailAddress, string templateId, Dictionary<string, dynamic> personalisation)
    {
        try {
            var response = await notificationClient.SendEmailAsync(
                recipientEmailAddress,
                templateId,
                personalisation
            );
        }
        catch (NotifyClientException ex) {
            //todo: could do with including masked emails to the logs
            logger.LogError(ex, "Failed to send email notification to using template {TemplateId}.", templateId);
            throw new NotificationGatewayException("Failed to send email notification.", ex);
        }
    }
}
