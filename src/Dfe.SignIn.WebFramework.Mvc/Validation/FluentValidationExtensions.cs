using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.Validation;

/// <summary>
/// A utility class to be used with the fluent validators
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Adds the error messages to the existing MVC model state
    /// </summary>
    /// <param name="result"></param>
    /// <param name="modelState"></param>
    /// <param name="prefix"></param>
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string? prefix = null)
    {
        if (result.IsValid) {
            return;
        }

        foreach (var error in result.Errors) {
            var key = string.IsNullOrEmpty(prefix)
                ? error.PropertyName
                : string.IsNullOrEmpty(error.PropertyName)
                    ? prefix
                    : $"{prefix}.{error.PropertyName}";

            modelState.AddModelError(key, error.ErrorMessage);
        }
    }

    /// <summary>
    /// Validates the model using the specified validator and returns the validation result
    /// </summary>
    /// <typeparam name="TValidator">The type of the validator</typeparam>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    public static async Task<ValidationResult> ValidateAsync<TValidator, TModel>(this TModel model)
        where TValidator : AbstractValidator<TModel>, new()
    {
        var validator = new TValidator();
        return await validator.ValidateAsync(model);
    }
}
