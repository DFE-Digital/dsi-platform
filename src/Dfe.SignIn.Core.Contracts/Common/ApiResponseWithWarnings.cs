using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.Core.Contracts.Common;

/// <summary>
/// Represents a response from an API that may contain warnings.
/// </summary>
public abstract record ApiResponseWithWarnings
{
    /// <summary>
    /// Gets the list of warnings associated with the response.
    /// </summary>
    public IReadOnlyList<Warning> Warnings { get; init; } = [];

    /// <summary>
    /// Determines whether the response contains a warning with the specified code.
    /// </summary>
    /// <param name="code">The code of the warning to check for.</param>
    /// <returns>true if the response contains the warning; otherwise, false.</returns>
    public bool HasWarning(string code)
     => this.Warnings.Any(w => w.Code == code);
}
