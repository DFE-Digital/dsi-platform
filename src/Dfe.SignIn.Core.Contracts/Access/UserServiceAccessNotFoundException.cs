using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// The exception thrown when a user does not have access to the requested service
/// within an organisation.
/// </summary>
public sealed class UserServiceAccessNotFoundException : NotFoundInteractionException
{
    /// <inheritdoc/>
    public UserServiceAccessNotFoundException() { }

    /// <inheritdoc/>
    public UserServiceAccessNotFoundException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public UserServiceAccessNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>The user ID that was not found.</summary>
    [Persist]
    public Guid? UserId { get; private set; }

    /// <summary>The service ID that was not found.</summary>
    [Persist]
    public Guid? ServiceId { get; private set; }

    /// <summary>The organisation ID that was not found.</summary>
    [Persist]
    public Guid? OrganisationId { get; private set; }

    /// <summary>
    /// Creates an instance of the exception from the IDs involved.
    /// </summary>
    public static UserServiceAccessNotFoundException From(Guid userId, Guid serviceId, Guid organisationId)
        => new() { UserId = userId, ServiceId = serviceId, OrganisationId = organisationId };
}
