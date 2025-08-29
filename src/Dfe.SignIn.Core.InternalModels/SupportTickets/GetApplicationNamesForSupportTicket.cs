namespace Dfe.SignIn.Core.InternalModels.SupportTickets;

/// <summary>
/// Request to get the list of application names which can be chosen when a user is
/// raising a support ticket.
/// </summary>
public sealed record GetApplicationNamesForSupportTicketRequest()
{
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationNamesForSupportTicketRequest"/>.
/// </summary>
public sealed record GetApplicationNamesForSupportTicketResponse()
{
    /// <summary>
    /// Gets the enumerable collection of applications.
    /// </summary>
    public required IEnumerable<ApplicationNameForSupportTicket> Applications { get; init; }
}

/// <summary>
/// Represents an application name result in a <see cref="GetApplicationNamesForSupportTicketResponse"/>.
/// </summary>
public sealed record ApplicationNameForSupportTicket()
{
    /// <summary>
    /// Gets the name of the service application.
    /// </summary>
    public required string Name { get; init; }
}
