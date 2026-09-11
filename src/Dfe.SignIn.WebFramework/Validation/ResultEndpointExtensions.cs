using Dfe.SignIn.Base.Framework.Results;

namespace Dfe.SignIn.WebFramework.Validation;

/// <summary>
/// Provides extension methods for converting <see cref="Result"/> and <see cref="Error"/> instances to validation problem results.
/// </summary>
public static class ResultEndpointExtensions
{
    /// <summary>
    /// Converts an <see cref="Error"/> instance to a validation problem result.
    /// </summary>
    /// <param name="error">The error instance to convert.</param>
    /// <param name="propertyName">The name of the property associated with the error.</param>
    /// <param name="type">A URI reference that identifies the problem type.</param>
    /// <returns>The validation problem result.</returns>
    public static IResult ToValidationProblem(this Error error, string? propertyName = null, string? type = null)
    {
        var field = propertyName ?? error.Target ?? string.Empty;

        return Results.ValidationProblem(
            new Dictionary<string, string[]> { [field] = [error.Description] },
            detail: error.Description,
            type: type ?? (string.IsNullOrEmpty(error.Code) ? null : error.Code));
    }

    /// <summary>
    /// Converts a <see cref="Result"/> instance to a validation problem result.
    /// </summary>
    /// <param name="result">The result instance to convert.</param>
    /// <param name="propertyName">The name of the property associated with the error.</param>
    /// <param name="type">A URI reference that identifies the problem type.</param>
    /// <returns>The validation problem result.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IResult ToValidationProblem(this Result result, string? propertyName = null, string? type = null)
    {
        if (result.IsSuccess) {
            throw new InvalidOperationException("Cannot convert a successful result to a validation problem.");
        }

        return result.Error.ToValidationProblem(propertyName, type);
    }
}
