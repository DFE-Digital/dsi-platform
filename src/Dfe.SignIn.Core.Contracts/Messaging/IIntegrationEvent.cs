namespace Dfe.SignIn.Core.Contracts.Messaging;

/// <summary>
/// Represents an integration event that can be published to a message bus or event stream.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}
