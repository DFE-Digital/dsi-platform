using Dfe.SignIn.Core.Contracts.SupportTickets;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// View model for the "Contact us" user interface.
/// </summary>
public sealed class ContactViewModel
{
    /// <summary>
    /// Gets or sets the title of the topic.
    /// </summary>
    [ValidateNever]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the optional brief summary of the topic.
    /// </summary>
    /// <remarks>
    ///   <para>This is plain text which should be rendered into an appropriate HTML
    ///   element such as a paragraph.</para>
    ///   <para>This text can also be used for description meta tags.</para>
    /// </remarks>
    [ValidateNever]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the HTML encoded body content of the topic.
    /// </summary>
    /// <remarks>
    ///   <para>This content should be presented as raw HTML since it is already HTML
    ///   encoded.</para>
    /// </remarks>
    [ValidateNever]
    public required string ContentHtml { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.FullName))]
    public string? FullNameInput { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.EmailAddress))]
    public string? EmailAddressInput { get; set; }

    /// <summary>
    /// Gets or sets the name of the user's organisation.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.OrganisationName))]
    public string? OrganisationNameInput { get; set; }

    /// <summary>
    /// Gets or sets the URN or UKPRN of the user's organisation.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.OrganisationUrn))]
    public string? OrganisationUrnInput { get; set; }

    /// <summary>
    /// Gets or sets the list of subjects to present in the contact form.
    /// </summary>
    [ValidateNever]
    public required IEnumerable<SubjectOptionForSupportTicket> SubjectOptions { get; set; }

    /// <summary>
    /// Gets or sets a subject that helps to indicate what the user needs help with.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.SubjectCode))]
    public string? SubjectCodeInput { get; set; }

    /// <summary>
    /// Gets or sets a custom subject that the user needs help with.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.CustomSummary))]
    public string? CustomSummaryInput { get; set; }

    /// <summary>
    /// Gets or sets the service that the user is using.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.ApplicationName))]
    public string? ApplicationNameInput { get; set; }

    /// <summary>
    /// Gets or sets the list of service options.
    /// </summary>
    [ValidateNever]
    public required IEnumerable<ApplicationNameForSupportTicket> ApplicationOptions { get; set; }

    /// <summary>
    /// Gets or sets the user message.
    /// </summary>
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.Message))]
    public string? MessageInput { get; set; }

    /// <summary>
    /// Trace identifier captured when an exception occurs. When a user follows the support link from an error page,
    /// this value is included so support can correlate logs and aid debugging.
    /// </summary>
    [ValidateNever]
    [MapTo<RaiseSupportTicketRequest>(nameof(RaiseSupportTicketRequest.ExceptionTraceId))]
    public string? ExceptionTraceId { get; set; }
}
