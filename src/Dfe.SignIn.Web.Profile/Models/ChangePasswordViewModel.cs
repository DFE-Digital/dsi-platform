using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;
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

    /// <summary>
    /// Gets a mapping of the properties of this view model to the properties of a <see cref="ChangePasswordRequest"/>.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> RequestPropertyMap = new Dictionary<string, string>() {
        [nameof(ChangePasswordRequest.CurrentPassword)] = nameof(CurrentPasswordInput),
        [nameof(ChangePasswordRequest.NewPassword)] = nameof(NewPasswordInput),
        [nameof(ChangePasswordRequest.ConfirmNewPassword)] = nameof(ConfirmNewPasswordInput),
    };
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
            .NotEmpty().WithMessage("Please enter a password")
            .MinimumLength(PasswordRequirements.MinimumLength).WithMessage("Please enter a valid password")
            .MaximumLength(PasswordRequirements.MaximumLength).WithMessage("Please enter a valid password")
            .Must(PasswordRequirements.MeetsComplexityRequirement).WithMessage("Please create a more secure password")
            .NotEqual(x => x.CurrentPasswordInput).WithMessage("Your new password cannot be one you have used recently.");

        this.RuleFor(x => x.ConfirmNewPasswordInput)
            .NotEmpty().WithMessage("Please enter a matching password")
            .Equal(x => x.NewPasswordInput).WithMessage("Please enter a matching password");
    }
}
