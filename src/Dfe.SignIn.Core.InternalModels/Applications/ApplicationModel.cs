namespace Dfe.SignIn.Core.InternalModels.Applications;

/// <summary>
/// A model representing a service application registration in DfE Sign-in.
/// </summary>
public sealed record ApplicationModel
{
    /// <summary>
    /// Gets the unique value that identifies the service application.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the unique client ID of the service application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the name of the service application.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a description of the application.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the home URL of the service.
    /// </summary>
    public Uri? ServiceHomeUrl { get; init; }

    /// <summary>
    /// The ApiSecret of the application.
    /// </summary>
    public string? ApiSecret { get; init; } = null;

    /// <summary>
    /// Gets a boolean value indicating if this is an external service.
    /// </summary>
    public required bool IsExternalService { get; init; }

    /// <summary>
    /// Gets a boolean value indicating if this is an ID-only service.
    /// </summary>
    public required bool IsIdOnlyService { get; init; }

    /// <summary>
    /// Gets a boolean value hinting that the service should be hidden on the
    /// "My Services" page if possible. Role based services are not hidden.
    /// </summary>
    public required bool IsHiddenService { get; init; }
}
