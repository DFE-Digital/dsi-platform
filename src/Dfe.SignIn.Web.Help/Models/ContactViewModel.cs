using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Web.Help.Validation;

namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// View model for the "Contact us" user interface.
/// </summary>
public sealed class ContactViewModel
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [Required(ErrorMessage = "Enter your name")]
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the name of the user's organisation.
    /// </summary>
    [Required(ErrorMessage = "Enter your organisation name")]
    public string? OrganisationName { get; set; }

    /// <summary>
    /// Gets or sets the URN or UKPRN of the user's organisation.
    /// </summary>
    [RegularExpression("^[0-9]{6,8}$", ErrorMessage = "Enter a valid URN or UKPRN, if known")]
    public string? OrganisationURN { get; set; }

    /// <summary>
    /// Gets the list of subjects to present in the contact form.
    /// </summary>
    public IEnumerable<ContactSubjectOptionViewModel> SubjectOptions => [
        new() { Value = "create-account", Text = "Setting up a DfE Sign-in account" },
        new() { Value = "service-access", Text = "Using a DfE service" },
        new() { Value = "email-password", Text = "Changing my email or password" },
        new() { Value = "deactivate-account", Text = "Deactivating my account" },
        new() { Value = "approver", Text = "Managing my users as an approver" },
        new() { Value = "add-org", Text = "Adding organisations to your account" },
        new() { Value = "other", Text = "Other (please specify)" },
    ];

    /// <summary>
    /// Gets or sets a subject that helps to indicate what the user needs help with.
    /// </summary>
    [Required(ErrorMessage = "Select the type of issue that you need help with")]
    public string? SelectedSubject { get; set; }

    /// <summary>
    /// Gets or sets a custom subject that the user needs help with.
    /// </summary>
    [RequiredIfTargetEquals(nameof(SelectedSubject), "other", ErrorMessage = "Enter a summary of your issue")]
    [MaxLength(200, ErrorMessage = "Issue summary must be 200 characters or less")]
    public string? CustomSummary { get; set; }

    /// <summary>
    /// Gets or sets the service that the user is using.
    /// </summary>
    [Required(ErrorMessage = "Select the service you are trying to use")]
    public string? SelectedService { get; set; }

    /// <summary>
    /// Gets or sets the list of service options.
    /// </summary>
    public required IEnumerable<string> ServiceOptions { get; set; }

    /// <summary>
    /// Gets or sets the user message.
    /// </summary>
    [Required(ErrorMessage = "Enter information about your issue")]
    [MaxLength(1000, ErrorMessage = "Issue details cannot be longer than 1000 characters")]
    public string? Message { get; set; }
}
