
namespace Dfe.SignIn.Core.Models.Applications.Interactions;

/// <summary>
/// Request to get an application by its unique client identifier.
/// </summary>
public record GetApplicationByClientIdRequest()
{
    /// <summary>
    /// Gets the unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationByClientIdRequest"/>.
/// </summary>
public record GetApplicationByClientIdResponse()
{
    /// <summary>
    /// Gets the application model
    /// otherwise, a value of <c>null</c>.
    /// </summary>
    public required ApplicationModel? Application { get; init; }
}
