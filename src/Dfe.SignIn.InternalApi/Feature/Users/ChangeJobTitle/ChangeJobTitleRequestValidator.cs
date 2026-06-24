using Dfe.SignIn.Core.Contracts;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using FluentValidation;

namespace Dfe.SignIn.InternalApi.Feature.Users.ChangeJobTitle;

/// <summary>
/// Validator for the <see cref="ChangeJobTitleRequest"/> class.
/// </summary>
public sealed class ChangeJobTitleRequestValidator : AbstractValidator<ChangeJobTitleRequest>
{
    /// <inheritdoc />
    public ChangeJobTitleRequestValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        this.RuleFor(x => x.NewJobTitle)
            .NotEmpty().WithMessage("NewJobTitle is required.")
            .MaximumLength(60).WithMessage("NewJobTitle must not exceed 60 characters");

        this.RuleFor(x => x.NewJobTitle)
            .Matches(StringPatterns.JobTitlePattern).WithMessage("NewJobTitle contains invalid characters.");
    }
}
