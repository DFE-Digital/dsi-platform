namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// A model representing the API client configuration of a service application
/// registration in DfE Sign-in.
/// </summary>
public sealed record ApplicationApiConfiguration
{
    /// <summary>
    /// Gets the unique client ID of the service application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// The secret that is required to validate the token of a relying party's request
    /// to the DfE Sign-In Public API.
    /// </summary>
    public required string ApiSecret { get; init; }
}
