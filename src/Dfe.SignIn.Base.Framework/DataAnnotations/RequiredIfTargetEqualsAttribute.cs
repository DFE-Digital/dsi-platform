using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Base.Framework.DataAnnotations;

/// <summary>
/// Specifies that a data field value is required when another data field has
/// a specific value.
/// </summary>
/// <param name="sourceProperty">Identifies the source property.</param>
/// <param name="sourceValue">The conditional value of the source property.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
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
            if (value is null || (value is string str && string.IsNullOrWhiteSpace(str))) {
                IEnumerable<string>? memberNames = validationContext.MemberName is not null
                    ? [validationContext.MemberName]
                    : null;
                return new ValidationResult(this.ErrorMessage, memberNames);
            }
        }

        return ValidationResult.Success;
    }
}
