using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Refit;

namespace Dfe.SignIn.WebFramework.Mvc.Validation;

/// <summary>
/// Provides extension methods for binding RFC 7807 ProblemDetails and ValidationProblemDetails
/// from Refit <see cref="IApiResponse"/> directly onto <see cref="ModelStateDictionary"/>.
/// </summary>
public static class ProblemDetailsMvcExtensions
{
    /// <summary>
    /// Checks if the response is a failure and extracts validation or general errors into <see cref="ModelStateDictionary"/>.
    /// </summary>
    /// <param name="response">The Refit API response.</param>
    /// <param name="modelState">The controller's model state dictionary.</param>
    /// <param name="propertyMap">Optional dictionary mapping API request field names to ViewModel property names.</param>
    /// <param name="fallbackField">The field to assign unmapped general problem details to (defaults to empty string for model-level).</param>
    /// <param name="defaultErrorMessage">Default error message if response has no content.</param>
    public static async Task TryAddProblemDetailsToModelStateAsync(
        this IApiResponse response,
        ModelStateDictionary modelState,
        IReadOnlyDictionary<string, string>? propertyMap = null,
        string fallbackField = "",
        string defaultErrorMessage = "We couldn't process your request right now. Please try again.")
    {
        if (response.Error is ApiException { HasContent: true } apiException) {
            // Try extracting RFC 7807 ValidationProblemDetails (key -> string[] errors)
            var validationProblem = await apiException.GetContentAsAsync<ValidationProblemDetails>();
            if (validationProblem?.Errors?.Count > 0) {
                foreach (var (apiField, messages) in validationProblem.Errors) {
                    var targetField = propertyMap?.GetValueOrDefault(apiField) ?? apiField;
                    foreach (var message in messages) {
                        modelState.AddModelError(targetField, message);
                    }
                }
            }

            // Fall back to standard ProblemDetails (single 'detail' string)
            var problem = await apiException.GetContentAsAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Detail)) {
                modelState.AddModelError(fallbackField, problem.Detail);
            }
        }

        modelState.AddModelError(string.Empty, defaultErrorMessage);
    }

    /// <summary>
    /// Safely extracts the 'detail' field from a standard ProblemDetails response without throwing.
    /// </summary>
    public static async Task<string?> GetDetailAsync(this IApiResponse response)
    {
        if (response.Error is not ApiException { HasContent: true } apiException) {
            return null;
        }

        try {
            var problem = await apiException.GetContentAsAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            return problem?.Detail;
        }
        catch {
            return null;
        }
    }
}
