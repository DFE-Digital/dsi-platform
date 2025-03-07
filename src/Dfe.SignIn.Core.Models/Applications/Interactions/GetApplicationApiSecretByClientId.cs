
namespace Dfe.SignIn.Core.Models.Applications.Interactions;

/// <summary>
/// Request to get an application ApiSecret by its unique client identifier.
/// </summary>
public record GetApplicationApiSecretByClientIdRequest
{
    /// <summary>
    /// Gets the unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationApiSecretByClientIdRequest"/>.
/// </summary>
public record GetApplicationApiSecretByClientIdResponse
{
    /// <summary>
    /// Gets a model representing the ApiSecret when the service was found,
    /// otherwise, a value of <c>null</c>.
    /// </summary>
    public required ApplicationApiSecretModel? Application { get; init; }
}