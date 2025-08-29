using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.InternalModels.Notifications;

/// <summary>
/// Request to send an email notification.
/// </summary>
public sealed record SendEmailNotificationRequest
{
    /// <summary>
    /// Gets the recipients email address.
    /// </summary>
    [Required, EmailAddress]
    public required string RecipientEmailAddress { get; init; }

    /// <summary>
    /// Gets the ID of the email template.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string TemplateId { get; init; }

    /// <summary>
    /// Gets the dictionary of template personalisation key/value pairs.
    /// </summary>
    public Dictionary<string, dynamic> Personalisation { get; init; } = [];
}

/// <summary>
/// Response model for interactor <see cref="SendEmailNotificationRequest"/>.
/// </summary>
public sealed record SendEmailNotificationResponse
{
}
