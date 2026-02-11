using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Notifications;

/// <summary>
/// Request to send an email notification.
/// </summary>
[AssociatedResponse(typeof(SendEmailNotificationResponse))]
public sealed record SendEmailNotificationRequest
{
    /// <summary>
    /// The recipients email address.
    /// </summary>
    [Required, EmailAddress]
    public required string RecipientEmailAddress { get; init; }

    /// <summary>
    /// The ID of the email template.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string TemplateId { get; init; }

    /// <summary>
    /// A dictionary of template personalisation key/value pairs.
    /// </summary>
    public Dictionary<string, dynamic> Personalisation { get; init; } = [];
}

/// <summary>
/// Response model for interactor <see cref="SendEmailNotificationRequest"/>.
/// </summary>
public sealed record SendEmailNotificationResponse
{
}
