namespace Dfe.SignIn.Base.Framework.Results;

/// <summary>
/// Represents an error with a code, description, and optional target.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Target">The target of the error, if applicable.</param>
public sealed record Error(string Code, string Description, string? Target = null)
{
    /// <summary>
    /// Represents a successful result with no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}

/// <summary>
/// Represents a warning with a code, description, and optional target.
/// </summary>
/// <param name="Code">The warning code.</param>
/// <param name="Description">The warning description.</param>
public sealed record Warning(string Code, string Description);
