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
            .MinimumLength(8).WithMessage("Please create a more secure password")
            .MaximumLength(64).WithMessage("Maximum length of password is 64 characters");

        this.RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Please confirm your new password")
            .Equal(x => x.NewPassword).WithMessage("Enter a matching password");

        this.RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Your new password cannot be one you have used recently");

        this.RuleFor(x => x.NewPassword)
            .Must(MeetsComplexityRequirement)
            .WithMessage("Please create a more secure password");
    }

    private static bool MeetsComplexityRequirement(string password)
    {
        int requirementsMet = 0;
        if (password.Any(char.IsLower)) {
            requirementsMet++;
        }

        if (password.Any(char.IsUpper)) {
            requirementsMet++;
        }

        if (password.Any(char.IsDigit)) {
            requirementsMet++;
        }

        if (password.Any(c => !char.IsLetterOrDigit(c))) {
            requirementsMet++;
        }

        return requirementsMet >= 3;
    }
}
