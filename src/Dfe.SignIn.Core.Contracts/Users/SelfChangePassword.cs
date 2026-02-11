using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to change a user's password.
/// </summary>
[AssociatedResponse(typeof(SelfChangePasswordResponse))]
[Throws(typeof(UserNotFoundException))]
public sealed partial record SelfChangePasswordRequest : IValidatableObject
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's access token for use with the Graph API.
    /// </summary>
    /// <remarks>
    ///   <para>This is only applicable for Entra user accounts.</para>
    /// </remarks>
    public GraphAccessToken? GraphAccessToken { get; init; } = null;

    /// <summary>
    /// The user's current password.
    /// </summary>
    [Required(ErrorMessage = "Please enter your current password")]
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// The user's new password.
    /// </summary>
    [Required(ErrorMessage = "Please enter your new password")]
    [MinLength(8, ErrorMessage = "Please create a more secure password")]
    [MaxLength(64, ErrorMessage = "Maximum length of password is 64 characters")]
    public required string NewPassword { get; init; }

    /// <summary>
    /// Confirmation of the user's new password.
    /// </summary>
    [Required(ErrorMessage = "Please confirm your new password")]
    public required string ConfirmNewPassword { get; init; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.NewPassword == this.CurrentPassword) {
            yield return new("Your new password cannot be one you have used recently", [nameof(this.NewPassword)]);
        }
        if (this.ConfirmNewPassword != this.NewPassword) {
            yield return new("Enter a matching password", [nameof(this.ConfirmNewPassword)]);
        }

        int requirementsMet = 0;
        if (AtLeastOneLowerCaseCharacterPattern().IsMatch(this.NewPassword)) {
            ++requirementsMet;
        }
        if (AtLeastOneUpperCaseCharacterPattern().IsMatch(this.NewPassword)) {
            ++requirementsMet;
        }
        if (AtLeastOneNumericCharacterPattern().IsMatch(this.NewPassword)) {
            ++requirementsMet;
        }
        if (AtLeastOneSymbolPattern().IsMatch(this.NewPassword)) {
            ++requirementsMet;
        }
        if (requirementsMet < 3) {
            yield return new("Please create a more secure password", [nameof(this.NewPassword)]);
        }
    }

    [GeneratedRegex("[a-z]")]
    private static partial Regex AtLeastOneLowerCaseCharacterPattern();
    [GeneratedRegex("[A-Z]")]
    private static partial Regex AtLeastOneUpperCaseCharacterPattern();
    [GeneratedRegex("[0-9]")]
    private static partial Regex AtLeastOneNumericCharacterPattern();
    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex AtLeastOneSymbolPattern();
}

/// <summary>
/// Represents a response for <see cref="SelfChangePasswordRequest"/>.
/// </summary>
public sealed record SelfChangePasswordResponse
{
}
