namespace Dfe.SignIn.Core.Contracts.SupportTickets;

/// <summary>
/// Request to get the list of subject options which can be chosen from when a
/// user is raising a support ticket.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetSubjectOptionsForSupportTicketResponse"/></item>
///   </list>
/// </remarks>
public sealed record GetSubjectOptionsForSupportTicketRequest
{
}

/// <summary>
/// Response model for interactor <see cref="GetSubjectOptionsForSupportTicketRequest"/>.
/// </summary>
public sealed record GetSubjectOptionsForSupportTicketResponse
{
    /// <summary>
    /// The enumerable collection of subject options.
    /// </summary>
    public required IEnumerable<SubjectOptionForSupportTicket> SubjectOptions { get; init; }
}

/// <summary>
/// Represents an application name result in a <see cref="GetSubjectOptionsForSupportTicketResponse"/>.
/// </summary>
public sealed record SubjectOptionForSupportTicket
{
    /// <summary>
    /// The unique code representing the subject option.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// A description of the subject option.
    /// </summary>
    public required string Description { get; init; }
}
