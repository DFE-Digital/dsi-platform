using FluentValidation;

namespace Dfe.SignIn.Core.Contracts.Features.Users.CheckIsBlockedEmail;

/// <summary>
/// Validator for check blocked email addresses
/// </summary>
public sealed class CheckIsBlockedEmailAddressRequestValidator : AbstractValidator<CheckIsBlockedEmailAddressRequest>
{
    /// <inheritdoc />
    public CheckIsBlockedEmailAddressRequestValidator()
    {
        this.RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .Matches(StringPatterns.EmailAddressPattern);
    }
}
