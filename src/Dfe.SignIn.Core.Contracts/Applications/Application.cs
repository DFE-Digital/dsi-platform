namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// A model representing a service application registration in DfE Sign-in.
/// </summary>
public sealed record Application
{
    /// <summary>
    /// The unique value that identifies the service application.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The unique client ID of the service application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// The name of the service application.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A description of the application.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The home URL of the service.
    /// </summary>
    public Uri? ServiceHomeUrl { get; init; }

    /// <summary>
    /// A boolean value indicating if this is an external service.
    /// </summary>
    public required bool IsExternalService { get; init; }

    /// <summary>
    /// A boolean value indicating if this is an ID-only service.
    /// </summary>
    public required bool IsIdOnlyService { get; init; }

    /// <summary>
    /// A boolean value hinting that the service should be hidden on the "My Services"
    /// page if possible. Role based services are not hidden.
    /// </summary>
    public required bool IsHiddenService { get; init; }
}
