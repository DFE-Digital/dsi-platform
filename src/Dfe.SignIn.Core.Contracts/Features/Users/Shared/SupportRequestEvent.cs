using Dfe.SignIn.Core.Contracts.Messaging;

namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents a support request.
/// </summary>
public sealed record SupportRequestEvent : IIntegrationEvent
{
    /// <summary>
    /// Name of the request
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The mailbox address to send to
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Service name
    /// </summary>
    public string? Service { get; set; }

    /// <summary>
    /// Type of support request
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Any additional supporting information
    /// </summary>
    public string? TypeAdditionalInfo { get; set; }

    /// <summary>
    /// Organisation name
    /// </summary>
    public string? OrgName { get; set; }

    /// <summary>
    /// Urn for the request
    /// </summary>
    public string? Urn { get; set; }

    /// <summary>
    /// Message body data
    /// </summary>
    public required string Message { get; set; }

    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
