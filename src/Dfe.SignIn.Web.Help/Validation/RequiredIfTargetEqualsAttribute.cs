using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Web.Help.Validation;

/// <summary>
/// Specifies that a data field value is required when another data field has
/// a specific value.
/// </summary>
/// <param name="sourceProperty">Identifies the source property.</param>
/// <param name="sourceValue">The conditional value of the source property.</param>
public sealed class RequiredIfTargetEqualsAttribute(
    string sourceProperty,
    object sourceValue
) : ValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(sourceProperty)
            ?? throw new MissingMemberException($"Unknown property: {sourceProperty}");

        var dependentValue = property.GetValue(validationContext.ObjectInstance, null);
        if (Equals(dependentValue, sourceValue)) {
            if (string.IsNullOrWhiteSpace(value?.ToString())) {
                return new ValidationResult(this.ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
