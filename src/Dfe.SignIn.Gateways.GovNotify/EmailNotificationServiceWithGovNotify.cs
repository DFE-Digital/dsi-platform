using Dfe.SignIn.Core.Contracts.Notifications;
using Microsoft.Extensions.Logging;
using Notify.Exceptions;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify;

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
    public async Task SendAsync(string recipientEmailAddress, string templateId, IReadOnlyDictionary<string, dynamic> personalisation)
    {
        try {
            await notificationClient.SendEmailAsync(
                recipientEmailAddress,
                templateId,
                personalisation.ToDictionary()
            );
        }
        catch (NotifyClientException ex) {
            logger.LogError(ex, "Failed to send email notification to using template {TemplateId}.", templateId);
            throw new NotificationGatewayException("Failed to send email notification.", ex);
        }
    }
}
