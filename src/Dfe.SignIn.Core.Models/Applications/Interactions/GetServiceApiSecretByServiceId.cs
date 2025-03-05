using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Core.Models.Applications.Interactions;

/// <summary>
/// Request model for interactor <see cref="IGetServiceApiSecretByServiceId"/>.
/// </summary>
public record GetServiceApiSecretByServiceIdRequest
{
    /// <summary>
    /// Gets the unique service identifier of the client.
    /// </summary>
    public required Guid ServiceId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="IGetServiceApiSecretByServiceId"/>.
/// </summary>
public record GetServiceApiSecretByServiceIdResponse
{
    /// <summary>
    /// Gets a model representing the service.
    /// </summary>
    public required ServiceApiSecretModel Service { get; init; }
}

/// <summary>
/// An interactor that gets a service with the given unique service identifier.
/// </summary>
[InteractorContract]
public interface IGetServiceApiSecretByServiceId
    : IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse>;
