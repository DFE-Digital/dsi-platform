using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Core.Models.Applications.Interactions;

/// <summary>
/// Request model for interactor <see cref="IGetApplicationByClientId"/>.
/// </summary>
public record GetApplicationByClientIdRequest
{
    /// <summary>
    /// Gets the unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="IGetApplicationByClientId"/>.
/// </summary>
public record GetApplicationByClientIdResponse
{
    /// <summary>
    /// Gets a model representing the application.
    /// </summary>
    public required ApplicationModel Application { get; init; }
}

/// <summary>
/// An interactor that gets an application with the given unique client identifier.
/// </summary>
[InteractorContract]
public interface IGetApplicationByClientId
    : IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>;
