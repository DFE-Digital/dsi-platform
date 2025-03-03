namespace Dfe.SignIn.Core.Models.Applications;

/// <summary>
/// A model representing an application registration in DfE Sign-in.
/// </summary>
public sealed record ApplicationModel
{
    /// <summary>
    /// Gets a unique value that identifies the application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    public required string Name { get; init; }
}
