using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Framework.DataAnnotations;

/// <summary>
/// Specifies that a data field value is required when another data field has
/// a specific value.
/// </summary>
/// <param name="sourceProperty">Identifies the source property.</param>
/// <param name="sourceValue">The conditional value of the source property.</param>
[ExcludeFromCodeCoverage] // (SonarQube does not associate with -> RequiredIfTargetEqualsAttributeTests)
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
                return new ValidationResult(this.ErrorMessage, [validationContext.MemberName]);
            }
        }

        return ValidationResult.Success;
    }
}
