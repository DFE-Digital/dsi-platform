using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Refit;

/// <summary>
/// Extensions for adding validation errors to a <see cref="ModelStateDictionary"/> from an <see cref="ApiException"/>.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Attempts to add validation errors from an <see cref="ApiException"/> to a <see cref="ModelStateDictionary"/>.
    /// </summary>
    /// <param name="modelState">The <see cref="ModelStateDictionary"/> to add errors to.</param>
    /// <param name="ex">The <see cref="ApiException"/> containing validation errors.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether any validation errors were added.</returns>
    public static async Task<bool> TryAddValidationErrorsAsync(
        this ModelStateDictionary modelState,
        ApiException ex)
    {
        if (ex.StatusCode != System.Net.HttpStatusCode.BadRequest) {
            return false;
        }

        // Deserialize standard RFC 7807 ValidationProblemDetails payload
        var problemDetails = await ex.GetContentAsAsync<ValidationProblemDetails>();
        if (problemDetails?.Errors is null) {
            return false;
        }

        foreach (var (key, errorMessages) in problemDetails.Errors) {
            foreach (var message in errorMessages) {
                // Key matches property name on the ViewModel
                modelState.AddModelError(key, message);
            }
        }

        return true;
    }
}
