using Dfe.SignIn.Core.Contracts.Messaging;

namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents an event that is published when a user is updated in the system.
/// </summary>
public sealed record UserUpdatedEvent : IIntegrationEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserUpdatedEvent"/> class.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the status of the user.
    /// </summary>
    public required short Status { get; set; } = 1;

    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
