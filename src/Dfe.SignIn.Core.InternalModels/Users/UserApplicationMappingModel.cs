namespace Dfe.SignIn.Core.InternalModels.Users;

/// <summary>
/// A model representing an application
/// </summary>
public sealed record UserApplicationMappingModel()
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }

    /// <summary>
    /// Unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// Date/Time of when access was granted.
    /// </summary>
    public required DateTime AccessGranted { get; init; }
}
