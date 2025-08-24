using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Extension methods for validation results.
/// </summary>
public static class ValidationResultsExtensions
{
    /// <summary>
    /// Adds validation results to the given model state.
    /// </summary>
    /// <param name="modelState">The model state to add to.</param>
    /// <param name="validationResults">An enumerable collection of validation results.</param>
    /// <param name="memberNameMappings">A dictionary that maps validation member names
    /// to the equivalent members of the view model.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="modelState"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="validationResults"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="memberNameMappings"/> is null.</para>
    /// </exception>
    public static void AddFrom(
        this ModelStateDictionary modelState,
        IEnumerable<ValidationResult> validationResults,
        Dictionary<string, string> memberNameMappings)
    {
        ExceptionHelpers.ThrowIfArgumentNull(modelState, nameof(modelState));
        ExceptionHelpers.ThrowIfArgumentNull(validationResults, nameof(validationResults));
        ExceptionHelpers.ThrowIfArgumentNull(memberNameMappings, nameof(memberNameMappings));

        foreach (var validationResult in validationResults) {
            foreach (var memberName in validationResult.MemberNames) {
                if (memberNameMappings.TryGetValue(memberName, out var mappedName)) {
                    modelState.AddModelError(mappedName, validationResult.ErrorMessage ?? $"Invalid value for {mappedName}");
                }
            }
        }
    }
}
