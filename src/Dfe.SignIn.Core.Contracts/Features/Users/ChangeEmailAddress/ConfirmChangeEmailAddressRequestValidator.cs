using FluentValidation;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Validator for the <see cref="ConfirmChangeEmailAddressRequest"/> class.
/// </summary>
public sealed class ConfirmChangeEmailAddressRequestValidator : AbstractValidator<ConfirmChangeEmailAddressRequest>
{
    /// <inheritdoc />
    public ConfirmChangeEmailAddressRequestValidator()
    {
        this.RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Please enter verification code");
    }
}
