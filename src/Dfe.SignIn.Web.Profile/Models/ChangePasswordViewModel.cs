using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their password.
/// </summary>
public sealed class ChangePasswordViewModel
{
    /// <summary>
    /// Gets or sets the current password of the user.
    /// </summary>
    [DataType(DataType.Password)]
    public string? CurrentPasswordInput { get; set; }

    /// <summary>
    /// Gets or sets the new password of the user.
    /// </summary>
    [DataType(DataType.Password)]
    public string? NewPasswordInput { get; set; }

    /// <summary>
    /// Gets or sets a confirmation of the user's new password.
    /// </summary>
    [DataType(DataType.Password)]
    public string? ConfirmNewPasswordInput { get; set; }
}

/// <summary>
/// Validator for the <see cref="ChangePasswordViewModel"/>.
/// </summary>
public sealed class ChangePasswordViewModelValidator : AbstractValidator<ChangePasswordViewModel>
{
    public ChangePasswordViewModelValidator()
    {
        this.RuleFor(x => x.CurrentPasswordInput)
            .NotEmpty().WithMessage("Please enter your current password");

        this.RuleFor(x => x.NewPasswordInput)
            .NotEmpty().WithMessage("Please enter a new password")
            .MinimumLength(8).WithMessage("Your password must be at least 8 characters")
            .MaximumLength(64).WithMessage("Your password must not exceed 64 characters")
            .Must(HasComplexity).WithMessage("Your password must contain at least 3 of these: uppercase letters, lowercase letters, numbers, special characters")
            .NotEqual(x => x.CurrentPasswordInput).WithMessage("Your new password cannot be the same as your current password");

        this.RuleFor(x => x.ConfirmNewPasswordInput)
            .NotEmpty().WithMessage("Please confirm your new password")
            .Equal(x => x.NewPasswordInput).WithMessage("Passwords do not match");
    }

    private static bool HasComplexity(string? password)
    {
        if (string.IsNullOrEmpty(password)) {
            return false;
        }

        int requirements = 0;
        if (password.Any(char.IsUpper)) requirements++;
        if (password.Any(char.IsLower)) requirements++;
        if (password.Any(char.IsDigit)) requirements++;
        if (password.Any(c => !char.IsLetterOrDigit(c))) requirements++;

        return requirements >= 3;
    }
}
