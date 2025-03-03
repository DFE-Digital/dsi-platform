namespace Dfe.SignIn.Core.Models.Applications.Interactions;

/// <summary>
/// Represents a request to get an application using the client identifier which
/// uniquely identifies the application.
/// </summary>
/// <seealso cref="GetApplicationByClientIdResponse"/>
public record GetApplicationByClientIdRequest
{
    /// <summary>
    /// Gets the unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Represents a response having requested <see cref="GetApplicationByClientIdRequest"/>.
/// </summary>
public record GetApplicationByClientIdResponse
{
    /// <summary>
    /// Gets a model representing the application.
    /// </summary>
    public required ApplicationModel Application { get; init; }
}
