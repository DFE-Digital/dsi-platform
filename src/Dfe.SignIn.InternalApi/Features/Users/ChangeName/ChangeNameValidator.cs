using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using FluentValidation;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeName;

/// <summary>
/// Validator for the <see cref="ChangeNameRequest"/> class.
/// </summary>
public sealed class ChangeNameRequestValidator : AbstractValidator<ChangeNameRequest>
{
    /// <inheritdoc />
    public ChangeNameRequestValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        this.RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Enter a first name")
            .MaximumLength(60).WithMessage("Enter a name with no more than 60 characters");

        this.RuleFor(x => x.FirstName)
            .Matches(StringPatterns.FirstNamePattern).WithMessage("Special characters cannot be used in first name");

        this.RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Enter a last name")
            .MaximumLength(60).WithMessage("Enter a name with no more than 60 characters");

        this.RuleFor(x => x.LastName)
            .Matches(StringPatterns.LastNamePattern).WithMessage("Special characters cannot be used in last name");
    }
}
