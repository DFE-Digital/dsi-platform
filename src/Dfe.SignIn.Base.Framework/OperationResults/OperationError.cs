namespace Dfe.SignIn.Base.Framework.OperationResults;

/// <summary>
/// Represents an error with a code, description, and optional target.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Target">The target of the error, if applicable.</param>
public sealed record OperationError(string Code, string Description, string? Target = null)
{
    /// <summary>
    /// Represents a successful result with no error.
    /// </summary>
    public static readonly OperationError None = new(string.Empty, string.Empty);
}
