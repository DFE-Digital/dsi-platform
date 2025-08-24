using Dfe.SignIn.Core.InternalModels.SupportTickets;

namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// View model for the "Contact us" user interface.
/// </summary>
public sealed class ContactViewModel
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the name of the user's organisation.
    /// </summary>
    public string? OrganisationName { get; set; }

    /// <summary>
    /// Gets or sets the URN or UKPRN of the user's organisation.
    /// </summary>
    public string? OrganisationURN { get; set; }

    /// <summary>
    /// Gets or sets the list of subjects to present in the contact form.
    /// </summary>
    public IEnumerable<SubjectOptionForSupportTicket> SubjectOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets a subject that helps to indicate what the user needs help with.
    /// </summary>
    public string? SubjectCode { get; set; }

    /// <summary>
    /// Gets or sets a custom subject that the user needs help with.
    /// </summary>
    public string? CustomSummary { get; set; }

    /// <summary>
    /// Gets or sets the service that the user is using.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the list of service options.
    /// </summary>
    public IEnumerable<ApplicationNameForSupportTicket> ApplicationOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the user message.
    /// </summary>
    public string? Message { get; set; }
}
