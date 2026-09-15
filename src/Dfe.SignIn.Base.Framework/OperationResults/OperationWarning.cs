namespace Dfe.SignIn.Base.Framework.OperationResults;

/// <summary>
/// Represents a warning with a code, description, and optional target.
/// </summary>
/// <param name="Code">The warning code.</param>
/// <param name="Description">The warning description.</param>
public sealed record OperationWarning(string Code, string Description);
