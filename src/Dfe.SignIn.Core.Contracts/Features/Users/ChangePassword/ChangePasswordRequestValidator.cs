using FluentValidation;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;

/// <summary>
/// Validator for the <see cref="ChangePasswordRequest"/> class.
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    /// <inheritdoc />
    public ChangePasswordRequestValidator()
    {
        this.RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Please enter your current password");

        this.RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Please enter your new password")
            .MinimumLength(PasswordRequirements.MinimumLength).WithMessage("Please create a more secure password")
            .MaximumLength(PasswordRequirements.MaximumLength).WithMessage("Maximum length of password is 64 characters");

        this.RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Please confirm your new password")
            .Equal(x => x.NewPassword).WithMessage("Enter a matching password");

        this.RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Your new password cannot be one you have used recently");

        this.RuleFor(x => x.NewPassword)
            .Must(PasswordRequirements.MeetsComplexityRequirement)
            .WithMessage("Please create a more secure password");
    }
}
