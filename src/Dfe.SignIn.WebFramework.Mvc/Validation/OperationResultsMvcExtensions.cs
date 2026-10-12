using Dfe.SignIn.Base.Framework.OperationResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.Validation;

/// <summary>
/// Extension methods for binding <see cref="OperationResult"/> errors to <see cref="ModelStateDictionary"/>.
/// </summary>
public static class OperationResultsMvcExtensions
{
    /// <summary>
    /// Adds the result's error to the ModelStateDictionary if the result is a failure.
    /// </summary>
    public static void AddToModelState(this OperationResult result, ModelStateDictionary modelState)
    {
        if (result.IsFailure && result.Error != OperationError.None) {
            modelState.AddModelError(result.Error.Target ?? string.Empty, result.Error.Description);
        }
    }
}
