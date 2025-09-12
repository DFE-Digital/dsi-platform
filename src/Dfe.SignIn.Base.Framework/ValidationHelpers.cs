using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Helper functionality for member validation.
/// </summary>
public static class ValidationHelpers
{
    #region Helpers for unit testing

    private static List<ValidationResult> ValidateForTest(object instance, bool expectFailure)
    {
        var results = new List<ValidationResult>();
        bool validationPassed = Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true
        );
        if (expectFailure && validationPassed) {
            throw new Exception("Expected validation to fail; but it passed!");
        }
        if (!expectFailure && !validationPassed) {
            throw new Exception("Expected validation to pass; but it failed!");
        }
        return results;
    }

    /// <summary>
    /// Validate an object with the expectation that the validation should fail.
    /// </summary>
    /// <remarks>
    ///   <para>This helper function is intended for use in unit tests only.</para>
    /// </remarks>
    /// <param name="instance">The object under test.</param>
    /// <returns>
    ///   <para>The list of validation results.</para>
    /// </returns>
    /// <exception cref="Exception">
    ///   <para>If the validation has passed unexpectedly.</para>
    /// </exception>
    public static List<ValidationResult> ValidateAndExpectFailure(object instance)
    {
        return ValidateForTest(instance, expectFailure: true);
    }

    /// <summary>
    /// Validate an object with the expectation that the validation should pass.
    /// </summary>
    /// <remarks>
    ///   <para>This helper function is intended for use in unit tests only.</para>
    /// </remarks>
    /// <param name="instance">The object under test.</param>
    /// <exception cref="Exception">
    ///   <para>If the validation has failed unexpectedly.</para>
    /// </exception>
    public static void ValidateAndExpectOk(object instance)
    {
        ValidateForTest(instance, expectFailure: false);
    }

    #endregion

    /// <summary>
    /// Determines whether the object specifies exactly one of the given members.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <param name="memberNames">The list of members that are to be considered.</param>
    /// <returns>
    ///   <para>True when the object specifies exactly one of the given members.</para>
    ///   <para>- or -</para>
    ///   <para>False when the object specifies none of the given members.</para>
    ///   <para>- or -</para>
    ///   <para>False when the object specifies more than one of the given members.</para>
    /// </returns>
    /// <exception cref="MissingMemberException">
    ///   <para>If a given member is not defined or non-public on the object.</para>
    /// </exception>
    public static bool HasExactlyOneMember(
        ValidationContext validationContext, params string[] memberNames)
    {
        int count = 0;
        foreach (string name in memberNames) {
            var prop = validationContext.ObjectType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
                ?? throw new MissingMemberException(validationContext.ObjectType.Name, name);
            var value = prop.GetValue(validationContext.ObjectInstance);
            if (value is not null && ++count > 1) {
                return false;
            }
        }
        return count == 1;
    }
}
