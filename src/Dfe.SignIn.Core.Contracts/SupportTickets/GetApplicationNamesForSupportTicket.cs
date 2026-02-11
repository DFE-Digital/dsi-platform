using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.SupportTickets;

/// <summary>
/// Request to get the list of application names which can be chosen when a user is
/// raising a support ticket.
/// </summary>
[AssociatedResponse(typeof(GetApplicationNamesForSupportTicketResponse))]
public sealed record GetApplicationNamesForSupportTicketRequest : IKeyedRequest
{
    /// <inheritdoc/>
    string IKeyedRequest.Key => KeyedRequestConstants.DefaultKey;
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationNamesForSupportTicketRequest"/>.
/// </summary>
public sealed record GetApplicationNamesForSupportTicketResponse
{
    /// <summary>
    /// The enumerable collection of applications.
    /// </summary>
    public required IEnumerable<ApplicationNameForSupportTicket> Applications { get; init; }
}

/// <summary>
/// Represents an application name result in a <see cref="GetApplicationNamesForSupportTicketResponse"/>.
/// </summary>
public sealed record ApplicationNameForSupportTicket
{
    /// <summary>
    /// The name of the service application.
    /// </summary>
    public required string Name { get; init; }
}
