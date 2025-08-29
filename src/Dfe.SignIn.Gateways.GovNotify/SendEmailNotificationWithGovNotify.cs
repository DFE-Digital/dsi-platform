using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Notifications;
using Notify.Interfaces;

namespace Dfe.SignIn.Gateways.GovNotify;

/// <summary>
/// An interactor that sends email notifications using GOV.UK Notify.
/// </summary>
/// <param name="notificationClient">The GOV.UK Notify client.</param>
public sealed class SendEmailNotificationWithGovNotify(
    IAsyncNotificationClient notificationClient
) : Interactor<SendEmailNotificationRequest, SendEmailNotificationResponse>
{
    /// <inheritdoc/>
    public override async Task<SendEmailNotificationResponse> InvokeAsync(
        InteractionContext<SendEmailNotificationRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        await notificationClient.SendEmailAsync(
            context.Request.RecipientEmailAddress,
            context.Request.TemplateId,
            context.Request.Personalisation
        );

        return new SendEmailNotificationResponse();
    }
}
