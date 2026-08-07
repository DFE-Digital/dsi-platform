using FluentValidation;

namespace Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;

/// <summary>
/// Validator for the <see cref="ChangeJobTitleRequest"/> class.
/// </summary>
public sealed class ChangeJobTitleRequestValidator : AbstractValidator<ChangeJobTitleRequest>
{
    /// <inheritdoc />
    public ChangeJobTitleRequestValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        this.RuleFor(x => x.NewJobTitle)
            .NotEmpty()
            .MaximumLength(60)
            .Matches(StringPatterns.JobTitlePattern)
            .WithMessage("Special characters cannot be used in job title.");
    }
}
