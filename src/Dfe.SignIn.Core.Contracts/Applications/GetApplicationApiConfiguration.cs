using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// Request to get the Public API client configuration for an application by its
/// unique client identifier.
/// </summary>
[AssociatedResponse(typeof(GetApplicationApiConfigurationResponse))]
[Throws(typeof(ApplicationNotFoundException))]
public sealed record GetApplicationApiConfigurationRequest : IKeyedRequest
{
    /// <summary>
    /// The unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <inheritdoc/>
    string IKeyedRequest.Key => this.ClientId;
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationApiConfigurationRequest"/>.
/// </summary>
public sealed record GetApplicationApiConfigurationResponse
{
    /// <summary>
    /// The API client configuration for the application.
    /// </summary>
    public required ApplicationApiConfiguration Configuration { get; init; }
}
