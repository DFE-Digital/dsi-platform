using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.Validation;

/// <summary>
/// Extension methods for binding <see cref="Result"/> errors to <see cref="ModelStateDictionary"/>.
/// </summary>
public static class ResultMvcExtensions
{
    /// <summary>
    /// Adds the result's error to the ModelStateDictionary if the result is a failure.
    /// </summary>
    public static void AddToModelState(this Result result, ModelStateDictionary modelState)
    {
        if (result.IsFailure && result.Error != Error.None) {
            modelState.AddModelError(result.Error.Target ?? string.Empty, result.Error.Description);
        }
    }
}
