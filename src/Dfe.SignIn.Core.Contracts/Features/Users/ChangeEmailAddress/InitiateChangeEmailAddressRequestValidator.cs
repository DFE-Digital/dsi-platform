using FluentValidation;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;

/// <summary>
/// Validator for the <see cref="InitiateChangeEmailAddressRequest"/> class.
/// </summary>
public sealed class InitiateChangeEmailAddressRequestValidator : AbstractValidator<InitiateChangeEmailAddressRequest>
{
    /// <inheritdoc />
    public InitiateChangeEmailAddressRequestValidator()
    {
        this.RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        this.RuleFor(x => x.NewEmailAddress)
            .NotEmpty().WithMessage("Enter an email address")
            .MaximumLength(UserConstants.MaxEmailAddressLength).WithMessage($"Enter an email address with no more than {UserConstants.MaxEmailAddressLength} characters")
            .Matches(StringPatterns.EmailAddressPattern).WithMessage("Enter a valid email address");
    }
}
