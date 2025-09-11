using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.SupportTickets;

/// <summary>
/// Request to raise a support ticket.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="RaiseSupportTicketResponse"/></item>
///   </list>
/// </remarks>
public sealed record RaiseSupportTicketRequest
{
    /// <summary>
    /// The full name of the user.
    /// </summary>
    [Required(ErrorMessage = "Enter your name")]
    public required string FullName { get; init; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// The name of the user's organisation.
    /// </summary>
    [Required(ErrorMessage = "Enter your organisation name")]
    public required string OrganisationName { get; init; }

    /// <summary>
    /// The URN or UKPRN of the user's organisation.
    /// </summary>
    [RegularExpression("^[0-9]{6,8}$", ErrorMessage = "Enter a valid URN or UKPRN, if known")]
    public string? OrganisationURN { get; init; } = null;

    /// <summary>
    /// The subject that helps to indicate what the user needs help with.
    /// </summary>
    [Required(ErrorMessage = "Select the type of issue that you need help with")]
    public required string SubjectCode { get; init; }

    /// <summary>
    /// The custom subject that the user needs help with.
    /// </summary>
    /// <remarks>
    ///   <para>This is relevant when <see cref="SubjectCode"/> has a value of "other".</para>
    /// </remarks>
    [RequiredIfTargetEquals(nameof(SubjectCode), "other", ErrorMessage = "Enter a summary of your issue")]
    [MaxLength(200, ErrorMessage = "Issue summary must be 200 characters or less")]
    public string? CustomSummary { get; init; } = null;

    /// <summary>
    /// The service that the user is using.
    /// </summary>
    [Required(ErrorMessage = "Select the service you are trying to use")]
    public required string ApplicationName { get; init; }

    /// <summary>
    /// The user message.
    /// </summary>
    [Required(ErrorMessage = "Enter information about your issue")]
    [MaxLength(1000, ErrorMessage = "Issue details cannot be longer than 1000 characters")]
    public required string Message { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="RaiseSupportTicketRequest"/>.
/// </summary>
public sealed record RaiseSupportTicketResponse
{
}
