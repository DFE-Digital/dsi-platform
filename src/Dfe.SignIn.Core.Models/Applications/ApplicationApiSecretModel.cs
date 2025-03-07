namespace Dfe.SignIn.Core.Models.Applications;

/// <summary>
/// A model representing an application ApiSecret in DfE Sign-in.
/// </summary>
public sealed record ApplicationApiSecretModel()
{
    /// <summary>
    /// Gets the ClientId for the application
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the ApiSecret for the application
    /// </summary>
    public string? ApiSecret { get; init; }
}
