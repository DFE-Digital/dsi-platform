using Dfe.SignIn.Core.Contracts;
using FluentValidation;

namespace Dfe.SignIn.Web.Profile.Models;

/// <summary>
/// View model for the view that allows a user to change their job title.
/// </summary>
public sealed class ChangeJobTitleViewModel
{
    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    public string? JobTitleInput { get; set; }
}

public sealed class ChangeJobTitleViewModelValidator : AbstractValidator<ChangeJobTitleViewModel>
{
    /// <inheritdoc />
    public ChangeJobTitleViewModelValidator()
    {
        this.RuleFor(x => x.JobTitleInput)
            .NotEmpty()
            .MaximumLength(60)
            .Matches(StringPatterns.JobTitlePattern)
            .WithMessage("Special characters cannot be used in job title.");
    }
}
